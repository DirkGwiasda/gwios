#include "bme280_compensation.h"
#include "unity.h"

void setUp(void)
{
}

void tearDown(void)
{
}

static void test_decodes_twenty_bit_pressure_and_temperature(void)
{
    // Pressure 415148 (0x655AC) and temperature 519888 (0x7EED0), each most significant byte first and
    // left-aligned in three registers.
    const uint8_t data[BME280_MEASUREMENT_DATA_LENGTH] = {0x65, 0x5A, 0xC0, 0x7E, 0xED, 0x00, 0x71, 0x48};
    bme280_raw_measurement_t raw;

    bme280_parse_raw_measurement(data, &raw);

    TEST_ASSERT_EQUAL_INT32(415148, raw.pressure);
    TEST_ASSERT_EQUAL_INT32(519888, raw.temperature);
}

static void test_decodes_sixteen_bit_humidity(void)
{
    const uint8_t data[BME280_MEASUREMENT_DATA_LENGTH] = {0x65, 0x5A, 0xC0, 0x7E, 0xED, 0x00, 0x71, 0x48};
    bme280_raw_measurement_t raw;

    bme280_parse_raw_measurement(data, &raw);

    TEST_ASSERT_EQUAL_INT32(29000, raw.humidity);
}

static void test_ignores_the_unused_low_nibble_of_pressure_and_temperature(void)
{
    const uint8_t data[BME280_MEASUREMENT_DATA_LENGTH] = {0x65, 0x5A, 0xCF, 0x7E, 0xED, 0x0F, 0x71, 0x48};
    bme280_raw_measurement_t raw;

    bme280_parse_raw_measurement(data, &raw);

    TEST_ASSERT_EQUAL_INT32(415148, raw.pressure);
    TEST_ASSERT_EQUAL_INT32(519888, raw.temperature);
}

int main(void)
{
    UNITY_BEGIN();
    RUN_TEST(test_decodes_twenty_bit_pressure_and_temperature);
    RUN_TEST(test_decodes_sixteen_bit_humidity);
    RUN_TEST(test_ignores_the_unused_low_nibble_of_pressure_and_temperature);
    return UNITY_END();
}
