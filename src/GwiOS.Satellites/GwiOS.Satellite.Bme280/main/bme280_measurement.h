#pragma once

#ifdef __cplusplus
extern "C" {
#endif

// One set of readings, already compensated with the factory calibration of the individual chip. Kept apart
// from bme280.h so that code without access to the I2C driver, such as the host tests, can use it.
typedef struct
{
    // Temperature in degrees Celsius.
    float temperature_celsius;

    // Relative humidity in percent, between 0 and 100.
    float humidity_percent;

    // Barometric pressure in hectopascal, uncorrected for altitude.
    float pressure_hectopascal;
} bme280_measurement_t;

#ifdef __cplusplus
}
#endif
