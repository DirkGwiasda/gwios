#include "bme280_compensation.h"
#include "main/bme280_compensation/datasheet_calibration.h"
#include "unity.h"

void setUp(void)
{
}

void tearDown(void)
{
}

// Floating-point reference formula from the BME280 datasheet, used as an independent oracle for the
// integer implementation. Returns percent relative humidity.
static double reference_humidity(const bme280_calibration_t *calibration, int32_t raw_humidity, int32_t t_fine)
{
    double value = (double)t_fine - 76800.0;

    value = (raw_humidity - (calibration->h4 * 64.0 + calibration->h5 / 16384.0 * value))
            * (calibration->h2 / 65536.0
               * (1.0 + calibration->h6 / 67108864.0 * value * (1.0 + calibration->h3 / 67108864.0 * value)));
    value = value * (1.0 - calibration->h1 * value / 524288.0);

    if (value > 100.0)
    {
        return 100.0;
    }

    if (value < 0.0)
    {
        return 0.0;
    }

    return value;
}

static void test_matches_the_reference_formula_for_a_typical_room(void)
{
    const bme280_calibration_t calibration = datasheet_calibration();
    const int32_t raw_humidity = 29000;

    const uint32_t humidity = bme280_compensate_humidity(&calibration, raw_humidity, DATASHEET_T_FINE);

    const double expected = reference_humidity(&calibration, raw_humidity, DATASHEET_T_FINE);
    TEST_ASSERT_DOUBLE_WITHIN(0.05, expected, humidity / 1024.0);
}

static void test_matches_the_reference_formula_for_dry_air(void)
{
    const bme280_calibration_t calibration = datasheet_calibration();
    const int32_t raw_humidity = 22000;

    const uint32_t humidity = bme280_compensate_humidity(&calibration, raw_humidity, DATASHEET_T_FINE);

    const double expected = reference_humidity(&calibration, raw_humidity, DATASHEET_T_FINE);
    TEST_ASSERT_DOUBLE_WITHIN(0.05, expected, humidity / 1024.0);
}

static void test_limits_the_result_to_zero_percent(void)
{
    const bme280_calibration_t calibration = datasheet_calibration();

    const uint32_t humidity = bme280_compensate_humidity(&calibration, 0, DATASHEET_T_FINE);

    TEST_ASSERT_EQUAL_UINT32(0, humidity);
}

static void test_limits_the_result_to_one_hundred_percent(void)
{
    const bme280_calibration_t calibration = datasheet_calibration();

    const uint32_t humidity = bme280_compensate_humidity(&calibration, 65535, DATASHEET_T_FINE);

    TEST_ASSERT_EQUAL_UINT32(100 * 1024, humidity);
}

int main(void)
{
    UNITY_BEGIN();
    RUN_TEST(test_matches_the_reference_formula_for_a_typical_room);
    RUN_TEST(test_matches_the_reference_formula_for_dry_air);
    RUN_TEST(test_limits_the_result_to_zero_percent);
    RUN_TEST(test_limits_the_result_to_one_hundred_percent);
    return UNITY_END();
}
