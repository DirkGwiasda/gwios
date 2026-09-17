#include "bme280_compensation.h"
#include "main/bme280_compensation/datasheet_calibration.h"
#include "unity.h"

void setUp(void)
{
}

void tearDown(void)
{
}

static void test_returns_pascal_times_256_for_the_datasheet_example(void)
{
    const bme280_calibration_t calibration = datasheet_calibration();

    const uint32_t pressure = bme280_compensate_pressure(&calibration, DATASHEET_RAW_PRESSURE, DATASHEET_T_FINE);

    TEST_ASSERT_EQUAL_UINT32(100653, pressure / 256);
}

static void test_returns_a_lower_pressure_for_a_higher_raw_value(void)
{
    const bme280_calibration_t calibration = datasheet_calibration();

    const uint32_t higher = bme280_compensate_pressure(&calibration, DATASHEET_RAW_PRESSURE, DATASHEET_T_FINE);
    const uint32_t lower = bme280_compensate_pressure(&calibration, DATASHEET_RAW_PRESSURE + 10000, DATASHEET_T_FINE);

    TEST_ASSERT_LESS_THAN_UINT32(higher, lower);
}

static void test_returns_zero_for_a_calibration_that_would_divide_by_zero(void)
{
    bme280_calibration_t calibration = datasheet_calibration();
    calibration.p1 = 0;

    const uint32_t pressure = bme280_compensate_pressure(&calibration, DATASHEET_RAW_PRESSURE, DATASHEET_T_FINE);

    TEST_ASSERT_EQUAL_UINT32(0, pressure);
}

int main(void)
{
    UNITY_BEGIN();
    RUN_TEST(test_returns_pascal_times_256_for_the_datasheet_example);
    RUN_TEST(test_returns_a_lower_pressure_for_a_higher_raw_value);
    RUN_TEST(test_returns_zero_for_a_calibration_that_would_divide_by_zero);
    return UNITY_END();
}
