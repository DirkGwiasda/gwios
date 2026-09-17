#include "bme280_compensation.h"
#include "main/bme280_compensation/datasheet_calibration.h"
#include "unity.h"

void setUp(void)
{
}

void tearDown(void)
{
}

static void test_returns_hundredths_of_a_degree_for_the_datasheet_example(void)
{
    const bme280_calibration_t calibration = datasheet_calibration();
    int32_t t_fine = 0;

    const int32_t temperature = bme280_compensate_temperature(&calibration, DATASHEET_RAW_TEMPERATURE, &t_fine);

    TEST_ASSERT_EQUAL_INT32(2508, temperature);
}

static void test_provides_t_fine_for_the_datasheet_example(void)
{
    const bme280_calibration_t calibration = datasheet_calibration();
    int32_t t_fine = 0;

    bme280_compensate_temperature(&calibration, DATASHEET_RAW_TEMPERATURE, &t_fine);

    TEST_ASSERT_EQUAL_INT32(DATASHEET_T_FINE, t_fine);
}

static void test_returns_a_higher_temperature_for_a_higher_raw_value(void)
{
    const bme280_calibration_t calibration = datasheet_calibration();
    int32_t t_fine = 0;

    const int32_t lower = bme280_compensate_temperature(&calibration, DATASHEET_RAW_TEMPERATURE, &t_fine);
    const int32_t higher = bme280_compensate_temperature(&calibration, DATASHEET_RAW_TEMPERATURE + 10000, &t_fine);

    TEST_ASSERT_GREATER_THAN_INT32(lower, higher);
}

int main(void)
{
    UNITY_BEGIN();
    RUN_TEST(test_returns_hundredths_of_a_degree_for_the_datasheet_example);
    RUN_TEST(test_provides_t_fine_for_the_datasheet_example);
    RUN_TEST(test_returns_a_higher_temperature_for_a_higher_raw_value);
    return UNITY_END();
}
