#include "bme280_compensation.h"

// Largest humidity value the compensation can produce before the final shift: 100 percent in Q22.10.
#define HUMIDITY_UPPER_LIMIT 419430400

static uint16_t to_unsigned_word(const uint8_t *little_endian_bytes)
{
    return (uint16_t)(((uint16_t)little_endian_bytes[1] << 8) | little_endian_bytes[0]);
}

static int16_t to_signed_word(const uint8_t *little_endian_bytes)
{
    return (int16_t)to_unsigned_word(little_endian_bytes);
}

// Pressure and temperature are 20-bit values spread over three registers: most significant byte first,
// the lowest four bits of the last register are unused.
static int32_t to_twenty_bit_value(const uint8_t *big_endian_bytes)
{
    return (int32_t)(((uint32_t)big_endian_bytes[0] << 12) | ((uint32_t)big_endian_bytes[1] << 4) | (big_endian_bytes[2] >> 4));
}

void bme280_parse_calibration(const uint8_t *first_block, const uint8_t *second_block, bme280_calibration_t *out_calibration)
{
    out_calibration->t1 = to_unsigned_word(&first_block[0]);
    out_calibration->t2 = to_signed_word(&first_block[2]);
    out_calibration->t3 = to_signed_word(&first_block[4]);
    out_calibration->p1 = to_unsigned_word(&first_block[6]);
    out_calibration->p2 = to_signed_word(&first_block[8]);
    out_calibration->p3 = to_signed_word(&first_block[10]);
    out_calibration->p4 = to_signed_word(&first_block[12]);
    out_calibration->p5 = to_signed_word(&first_block[14]);
    out_calibration->p6 = to_signed_word(&first_block[16]);
    out_calibration->p7 = to_signed_word(&first_block[18]);
    out_calibration->p8 = to_signed_word(&first_block[20]);
    out_calibration->p9 = to_signed_word(&first_block[22]);
    // Byte 24 (register 0xA0) is reserved.
    out_calibration->h1 = first_block[25];

    out_calibration->h2 = to_signed_word(&second_block[0]);
    out_calibration->h3 = second_block[2];

    // H4 and H5 are signed twelve-bit values that share the two nibbles of register 0xE5.
    out_calibration->h4 = (int16_t)(((int8_t)second_block[3] << 4) | (second_block[4] & 0x0F));
    out_calibration->h5 = (int16_t)(((int8_t)second_block[5] << 4) | (second_block[4] >> 4));
    out_calibration->h6 = (int8_t)second_block[6];
}

void bme280_parse_raw_measurement(const uint8_t *data, bme280_raw_measurement_t *out_raw)
{
    out_raw->pressure = to_twenty_bit_value(&data[0]);
    out_raw->temperature = to_twenty_bit_value(&data[3]);
    out_raw->humidity = (int32_t)(((uint32_t)data[6] << 8) | data[7]);
}

// The three compensation formulas below are the integer reference implementation from the BME280
// datasheet, kept as close to it as possible so they can be compared line by line.
int32_t bme280_compensate_temperature(const bme280_calibration_t *calibration, int32_t raw_temperature, int32_t *out_t_fine)
{
    int32_t part1 = ((((raw_temperature >> 3) - ((int32_t)calibration->t1 << 1))) * ((int32_t)calibration->t2)) >> 11;
    int32_t part2 = (((((raw_temperature >> 4) - ((int32_t)calibration->t1))
                       * ((raw_temperature >> 4) - ((int32_t)calibration->t1)))
                      >> 12)
                     * ((int32_t)calibration->t3))
                    >> 14;

    *out_t_fine = part1 + part2;

    return (*out_t_fine * 5 + 128) >> 8;
}

uint32_t bme280_compensate_pressure(const bme280_calibration_t *calibration, int32_t raw_pressure, int32_t t_fine)
{
    int64_t part1 = (int64_t)t_fine - 128000;
    int64_t part2 = part1 * part1 * (int64_t)calibration->p6;

    part2 = part2 + ((part1 * (int64_t)calibration->p5) << 17);
    part2 = part2 + (((int64_t)calibration->p4) << 35);
    part1 = ((part1 * part1 * (int64_t)calibration->p3) >> 8) + ((part1 * (int64_t)calibration->p2) << 12);
    part1 = (((((int64_t)1) << 47) + part1)) * ((int64_t)calibration->p1) >> 33;

    if (part1 == 0)
    {
        return 0;
    }

    int64_t pressure = 1048576 - raw_pressure;
    pressure = (((pressure << 31) - part2) * 3125) / part1;
    part1 = (((int64_t)calibration->p9) * (pressure >> 13) * (pressure >> 13)) >> 25;
    part2 = (((int64_t)calibration->p8) * pressure) >> 19;
    pressure = ((pressure + part1 + part2) >> 8) + (((int64_t)calibration->p7) << 4);

    return (uint32_t)pressure;
}

uint32_t bme280_compensate_humidity(const bme280_calibration_t *calibration, int32_t raw_humidity, int32_t t_fine)
{
    int32_t value = t_fine - (int32_t)76800;

    value = (((((raw_humidity << 14) - (((int32_t)calibration->h4) << 20) - (((int32_t)calibration->h5) * value))
               + ((int32_t)16384))
              >> 15)
             * (((((((value * ((int32_t)calibration->h6)) >> 10)
                    * (((value * ((int32_t)calibration->h3)) >> 11) + ((int32_t)32768)))
                   >> 10)
                  + ((int32_t)2097152))
                     * ((int32_t)calibration->h2)
                 + 8192)
                >> 14));

    value = value - (((((value >> 15) * (value >> 15)) >> 7) * ((int32_t)calibration->h1)) >> 4);

    if (value < 0)
    {
        value = 0;
    }

    if (value > HUMIDITY_UPPER_LIMIT)
    {
        value = HUMIDITY_UPPER_LIMIT;
    }

    return (uint32_t)(value >> 12);
}
