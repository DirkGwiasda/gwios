#include <stdbool.h>
#include <stdio.h>

#include "bme280.h"
#include "driver/i2c_master.h"
#include "esp_err.h"
#include "esp_log.h"
#include "freertos/FreeRTOS.h"
#include "freertos/task.h"
#include "measurement_server.h"
#include "measurement_store.h"
#include "ping_client.h"
#include "satellite_config.h"
#include "wifi_connection.h"

// Board and wiring of the BME280 satellite, documented in docs/devplan.md. The pins only fit this board,
// which is why its name is fixed here as well; it is reported to GwiOS in every ping.
#define SATELLITE_BOARD_NAME "ESP32 DevKit C"
#define SATELLITE_I2C_PORT I2C_NUM_0
#define SATELLITE_SDA_GPIO GPIO_NUM_21
#define SATELLITE_SCL_GPIO GPIO_NUM_22

#define MEASUREMENT_INTERVAL_MS 5000

// Prefix that makes satellites easy to spot in the device list of the router.
#define HOSTNAME_PREFIX "gwios-"

static const char *TAG = "satellite";

static i2c_master_bus_handle_t create_bus(void)
{
    i2c_master_bus_config_t configuration = {
        .i2c_port = SATELLITE_I2C_PORT,
        .sda_io_num = SATELLITE_SDA_GPIO,
        .scl_io_num = SATELLITE_SCL_GPIO,
        .clk_source = I2C_CLK_SRC_DEFAULT,
        .glitch_ignore_cnt = 7,
        // The sensor module carries its own 4.7 kOhm pull-ups. The internal ones are weak
        // and only keep the bus defined while nothing is plugged in.
        .flags.enable_internal_pullup = true,
    };

    i2c_master_bus_handle_t bus = NULL;
    ESP_ERROR_CHECK(i2c_new_master_bus(&configuration, &bus));

    return bus;
}

static void report_measurement(const bme280_measurement_t *measurement)
{
    ESP_LOGI(TAG,
             "%.2f C, %.2f %%rH, %.2f hPa",
             measurement->temperature_celsius,
             measurement->humidity_percent,
             measurement->pressure_hectopascal);
}

// Without a valid configuration the satellite still measures and logs locally, so a freshly flashed board
// stays usable and shows right away what is missing. Only the network part depends on the configuration.
static bool load_configuration(satellite_config_t *out_config)
{
    if (satellite_config_load(out_config) != ESP_OK)
    {
        ESP_LOGW(TAG, "No valid configuration, network disabled. Measuring locally only.");

        return false;
    }

    ESP_LOGI(TAG, "Configured as '%s', reporting to %s.", out_config->name, out_config->gwios_url);

    return true;
}

// A failure in the network part is logged but does not stop the satellite: it keeps measuring locally.
static void start_network(const satellite_config_t *config)
{
    char hostname[sizeof(HOSTNAME_PREFIX) + SATELLITE_NAME_MAX_LENGTH];
    snprintf(hostname, sizeof(hostname), HOSTNAME_PREFIX "%s", config->name);

    esp_err_t result = wifi_connection_start(hostname, config->wifi_ssid, config->wifi_password);
    if (result != ESP_OK)
    {
        ESP_LOGE(TAG, "Wi-Fi could not be started: %s. Measuring locally only.", esp_err_to_name(result));

        return;
    }

    // The server listens on all interfaces, so it can start before the connection is up.
    result = measurement_server_start(config->token);
    if (result != ESP_OK)
    {
        ESP_LOGE(TAG, "Measurement server could not be started: %s.", esp_err_to_name(result));
    }

    result = ping_client_start(config, SATELLITE_BOARD_NAME);
    if (result != ESP_OK)
    {
        ESP_LOGE(TAG, "Ping client could not be started: %s.", esp_err_to_name(result));
    }
}

static void measure(bme280_handle_t sensor)
{
    bme280_measurement_t measurement;
    esp_err_t result = bme280_measure(sensor, &measurement);
    if (result != ESP_OK)
    {
        ESP_LOGE(TAG, "Measurement failed: %s.", esp_err_to_name(result));

        return;
    }

    report_measurement(&measurement);
    measurement_store_put(&measurement);
}

void app_main(void)
{
    ESP_ERROR_CHECK(measurement_store_init());

    // Static because the configuration is needed for the whole lifetime of the firmware and is too large
    // to keep on the stack of the main task.
    static satellite_config_t config;
    if (load_configuration(&config))
    {
        start_network(&config);
    }

    i2c_master_bus_handle_t bus = create_bus();

    bme280_handle_t sensor = NULL;
    ESP_ERROR_CHECK(bme280_create(bus, BME280_ADDRESS_HIGH, &sensor));

    ESP_LOGI(TAG, "Sensor ready on SDA=GPIO%d, SCL=GPIO%d.", SATELLITE_SDA_GPIO, SATELLITE_SCL_GPIO);

    while (true)
    {
        measure(sensor);
        vTaskDelay(pdMS_TO_TICKS(MEASUREMENT_INTERVAL_MS));
    }
}
