#include "bme280.h"

#include <stdlib.h>

#include "bme280_compensation.h"
#include "esp_log.h"
#include "freertos/FreeRTOS.h"
#include "freertos/task.h"

#define I2C_SPEED_HZ 100000
#define I2C_TIMEOUT_MS 100

#define REGISTER_CALIBRATION_FIRST 0x88
#define REGISTER_CHIP_ID 0xD0
#define REGISTER_CALIBRATION_SECOND 0xE1
#define REGISTER_CONTROL_HUMIDITY 0xF2
#define REGISTER_STATUS 0xF3
#define REGISTER_CONTROL_MEASUREMENT 0xF4
#define REGISTER_CONFIG 0xF5
#define REGISTER_MEASUREMENT_DATA 0xF7

#define CHIP_ID_BME280 0x60

// Oversampling x1 on all three quantities, no filter, one measurement on demand. This is the
// setting Bosch recommends for weather monitoring and the one that draws the least power.
#define OVERSAMPLING_X1 0x01
#define MODE_FORCED 0x01
#define CONFIG_NO_FILTER 0x00

#define STATUS_MEASURING_BIT 0x08

// At oversampling x1 a measurement takes at most 9.3 ms. Waiting that long before looking at the
// status register also avoids reading the status before the sensor has even started.
#define MEASUREMENT_DURATION_MS 10
#define MEASUREMENT_POLL_ATTEMPTS 10
#define MEASUREMENT_POLL_INTERVAL_MS 5

static const char *TAG = "bme280";

struct bme280_sensor
{
    i2c_master_dev_handle_t device;

    // Factory calibration of this individual chip, read once when the sensor is claimed.
    bme280_calibration_t calibration;
};

static esp_err_t read_registers(bme280_handle_t sensor, uint8_t first_register, uint8_t *buffer, size_t length)
{
    return i2c_master_transmit_receive(sensor->device, &first_register, 1, buffer, length, I2C_TIMEOUT_MS);
}

static esp_err_t write_register(bme280_handle_t sensor, uint8_t target_register, uint8_t value)
{
    const uint8_t payload[] = {target_register, value};

    return i2c_master_transmit(sensor->device, payload, sizeof(payload), I2C_TIMEOUT_MS);
}

static esp_err_t verify_chip_id(bme280_handle_t sensor)
{
    uint8_t chip_id = 0;
    esp_err_t result = read_registers(sensor, REGISTER_CHIP_ID, &chip_id, sizeof(chip_id));
    if (result != ESP_OK)
    {
        return result;
    }

    if (chip_id != CHIP_ID_BME280)
    {
        ESP_LOGE(TAG, "Chip id 0x%02X is not a BME280 (expected 0x%02X).", chip_id, CHIP_ID_BME280);

        return ESP_ERR_NOT_FOUND;
    }

    return ESP_OK;
}

static esp_err_t read_calibration(bme280_handle_t sensor)
{
    // Registers 0x88 to 0xA1 hold the temperature and pressure calibration plus the first humidity
    // value; 0xE1 to 0xE7 hold the rest of the humidity calibration.
    uint8_t first_block[BME280_CALIBRATION_FIRST_BLOCK_LENGTH];
    esp_err_t result = read_registers(sensor, REGISTER_CALIBRATION_FIRST, first_block, sizeof(first_block));
    if (result != ESP_OK)
    {
        return result;
    }

    uint8_t second_block[BME280_CALIBRATION_SECOND_BLOCK_LENGTH];
    result = read_registers(sensor, REGISTER_CALIBRATION_SECOND, second_block, sizeof(second_block));
    if (result != ESP_OK)
    {
        return result;
    }

    bme280_parse_calibration(first_block, second_block, &sensor->calibration);

    return ESP_OK;
}

static esp_err_t start_measurement(bme280_handle_t sensor)
{
    // The humidity setting only takes effect once ctrl_meas is written. Writing the two registers
    // the other way round silently leaves humidity oversampling at its previous value.
    esp_err_t result = write_register(sensor, REGISTER_CONTROL_HUMIDITY, OVERSAMPLING_X1);
    if (result != ESP_OK)
    {
        return result;
    }

    const uint8_t control = (uint8_t)((OVERSAMPLING_X1 << 5) | (OVERSAMPLING_X1 << 2) | MODE_FORCED);

    return write_register(sensor, REGISTER_CONTROL_MEASUREMENT, control);
}

static esp_err_t wait_for_measurement(bme280_handle_t sensor)
{
    vTaskDelay(pdMS_TO_TICKS(MEASUREMENT_DURATION_MS));

    for (int attempt = 0; attempt < MEASUREMENT_POLL_ATTEMPTS; attempt++)
    {
        uint8_t status = 0;
        esp_err_t result = read_registers(sensor, REGISTER_STATUS, &status, sizeof(status));
        if (result != ESP_OK)
        {
            return result;
        }

        if ((status & STATUS_MEASURING_BIT) == 0)
        {
            return ESP_OK;
        }

        vTaskDelay(pdMS_TO_TICKS(MEASUREMENT_POLL_INTERVAL_MS));
    }

    return ESP_ERR_TIMEOUT;
}

esp_err_t bme280_measure(bme280_handle_t sensor, bme280_measurement_t *out_measurement)
{
    esp_err_t result = start_measurement(sensor);
    if (result != ESP_OK)
    {
        return result;
    }

    result = wait_for_measurement(sensor);
    if (result != ESP_OK)
    {
        return result;
    }

    // Pressure, temperature and humidity sit in eight consecutive registers and are read in one
    // burst, so all three values belong to the same measurement.
    uint8_t data[BME280_MEASUREMENT_DATA_LENGTH];
    result = read_registers(sensor, REGISTER_MEASUREMENT_DATA, data, sizeof(data));
    if (result != ESP_OK)
    {
        return result;
    }

    bme280_raw_measurement_t raw;
    bme280_parse_raw_measurement(data, &raw);

    int32_t t_fine = 0;
    const int32_t temperature = bme280_compensate_temperature(&sensor->calibration, raw.temperature, &t_fine);

    // Scale the fixed-point results: hundredths of a degree, Pascal times 256 to hectopascal, and
    // percent times 1024.
    out_measurement->temperature_celsius = temperature / 100.0f;
    out_measurement->pressure_hectopascal = bme280_compensate_pressure(&sensor->calibration, raw.pressure, t_fine) / 25600.0f;
    out_measurement->humidity_percent = bme280_compensate_humidity(&sensor->calibration, raw.humidity, t_fine) / 1024.0f;

    return ESP_OK;
}

esp_err_t bme280_create(i2c_master_bus_handle_t bus, uint16_t address, bme280_handle_t *out_sensor)
{
    bme280_handle_t sensor = calloc(1, sizeof(struct bme280_sensor));
    if (sensor == NULL)
    {
        return ESP_ERR_NO_MEM;
    }

    const i2c_device_config_t device_config = {
        .dev_addr_length = I2C_ADDR_BIT_LEN_7,
        .device_address = address,
        .scl_speed_hz = I2C_SPEED_HZ,
    };

    esp_err_t result = i2c_master_bus_add_device(bus, &device_config, &sensor->device);
    if (result != ESP_OK)
    {
        free(sensor);

        return result;
    }

    result = verify_chip_id(sensor);
    if (result != ESP_OK)
    {
        bme280_destroy(sensor);

        return result;
    }

    result = read_calibration(sensor);
    if (result != ESP_OK)
    {
        bme280_destroy(sensor);

        return result;
    }

    result = write_register(sensor, REGISTER_CONFIG, CONFIG_NO_FILTER);
    if (result != ESP_OK)
    {
        bme280_destroy(sensor);

        return result;
    }

    *out_sensor = sensor;

    return ESP_OK;
}

void bme280_destroy(bme280_handle_t sensor)
{
    if (sensor == NULL)
    {
        return;
    }

    i2c_master_bus_rm_device(sensor->device);
    free(sensor);
}
