#pragma once

#include <stdint.h>

#include "bme280_measurement.h"
#include "driver/i2c_master.h"
#include "esp_err.h"

#ifdef __cplusplus
extern "C" {
#endif

// The two addresses a BME280 can occupy. Which one applies is decided by the ADDR pin of the
// module: low gives 0x76, high gives 0x77.
#define BME280_ADDRESS_LOW 0x76
#define BME280_ADDRESS_HIGH 0x77

// Handle to one sensor on an I2C bus. The layout is private to the driver.
typedef struct bme280_sensor *bme280_handle_t;

// Claims the sensor at the given address on the given bus, verifies that the chip really is a
// BME280 and reads its factory calibration. Returns ESP_ERR_NOT_FOUND when another chip answers,
// which is what a BMP280 sold as a BME280 looks like.
esp_err_t bme280_create(i2c_master_bus_handle_t bus, uint16_t address, bme280_handle_t *out_sensor);

// Triggers a single measurement, waits for the sensor to finish it and returns the compensated
// values. Between two calls the sensor sleeps, which is what keeps its power draw low.
esp_err_t bme280_measure(bme280_handle_t sensor, bme280_measurement_t *out_measurement);

// Releases the sensor and removes it from its I2C bus.
void bme280_destroy(bme280_handle_t sensor);

#ifdef __cplusplus
}
#endif
