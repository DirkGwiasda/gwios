#pragma once

#include <stdint.h>

#ifdef __cplusplus
extern "C" {
#endif

// Number of bytes in the two calibration blocks and in one burst of measurement data, as laid out in
// the BME280 register map.
#define BME280_CALIBRATION_FIRST_BLOCK_LENGTH 26
#define BME280_CALIBRATION_SECOND_BLOCK_LENGTH 7
#define BME280_MEASUREMENT_DATA_LENGTH 8

// Factory calibration of one individual chip. The names follow the datasheet (dig_T1 becomes t1).
typedef struct
{
    uint16_t t1;
    int16_t t2;
    int16_t t3;
    uint16_t p1;
    int16_t p2;
    int16_t p3;
    int16_t p4;
    int16_t p5;
    int16_t p6;
    int16_t p7;
    int16_t p8;
    int16_t p9;
    uint8_t h1;
    int16_t h2;
    uint8_t h3;
    int16_t h4;
    int16_t h5;
    int8_t h6;
} bme280_calibration_t;

// Uncompensated readings as the sensor delivers them.
typedef struct
{
    int32_t temperature;
    int32_t pressure;
    int32_t humidity;
} bme280_raw_measurement_t;

// Decodes the calibration from the register blocks starting at 0x88 (26 bytes) and 0xE1 (7 bytes).
void bme280_parse_calibration(const uint8_t *first_block, const uint8_t *second_block, bme280_calibration_t *out_calibration);

// Decodes one burst of the eight measurement registers starting at 0xF7.
void bme280_parse_raw_measurement(const uint8_t *data, bme280_raw_measurement_t *out_raw);

// Returns the temperature in hundredths of a degree Celsius. t_fine receives the intermediate value that
// pressure and humidity compensation depend on.
int32_t bme280_compensate_temperature(const bme280_calibration_t *calibration, int32_t raw_temperature, int32_t *out_t_fine);

// Returns the pressure in Pascal as a fixed-point number with 8 fractional bits (divide by 256 for Pa).
// Returns 0 for a calibration that would divide by zero.
uint32_t bme280_compensate_pressure(const bme280_calibration_t *calibration, int32_t raw_pressure, int32_t t_fine);

// Returns the relative humidity in percent as a fixed-point number with 10 fractional bits (divide by
// 1024 for percent), limited to 0 to 100 percent.
uint32_t bme280_compensate_humidity(const bme280_calibration_t *calibration, int32_t raw_humidity, int32_t t_fine);

#ifdef __cplusplus
}
#endif
