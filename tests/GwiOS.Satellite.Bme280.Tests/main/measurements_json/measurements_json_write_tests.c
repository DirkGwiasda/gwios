#include <string.h>

#include "measurements_json.h"
#include "unity.h"

void setUp(void)
{
}

void tearDown(void)
{
}

static const bme280_measurement_t MEASUREMENT = {
    .temperature_celsius = 24.2f,
    .humidity_percent = 50.64f,
    .pressure_hectopascal = 996.37f,
};

// The exact document agreed with GwiOS in docs/devplan.md, with the degree sign as UTF-8.
static const char EXPECTED[] =
    "{\"measurements\":["
    "{\"sensor\":\"bme280\",\"quantity\":\"temperature\",\"value\":24.20,\"unit\":\"\xC2\xB0" "C\"},"
    "{\"sensor\":\"bme280\",\"quantity\":\"humidity\",\"value\":50.64,\"unit\":\"%\"},"
    "{\"sensor\":\"bme280\",\"quantity\":\"pressure\",\"value\":996.37,\"unit\":\"hPa\"}"
    "],\"ageSeconds\":3}";

static void test_writes_the_agreed_document(void)
{
    char buffer[512];

    measurements_json_write(buffer, sizeof(buffer), &MEASUREMENT, 3);

    TEST_ASSERT_EQUAL_STRING(EXPECTED, buffer);
}

static void test_returns_the_length_of_the_document(void)
{
    char buffer[512];

    const int length = measurements_json_write(buffer, sizeof(buffer), &MEASUREMENT, 3);

    TEST_ASSERT_EQUAL_INT((int)strlen(EXPECTED), length);
}

static void test_writes_negative_temperatures_with_sign(void)
{
    const bme280_measurement_t frost = {.temperature_celsius = -3.5f, .humidity_percent = 80.0f, .pressure_hectopascal = 1020.0f};
    char buffer[512];

    measurements_json_write(buffer, sizeof(buffer), &frost, 0);

    TEST_ASSERT_NOT_NULL(strstr(buffer, "\"value\":-3.50,"));
}

static void test_writes_the_age_in_seconds(void)
{
    char buffer[512];

    measurements_json_write(buffer, sizeof(buffer), &MEASUREMENT, 4294967295u);

    TEST_ASSERT_NOT_NULL(strstr(buffer, "\"ageSeconds\":4294967295}"));
}

static void test_fits_into_the_buffer_of_the_server(void)
{
    // Worst case for the length: long negative values and the largest age. measurement_server.c uses 320 bytes.
    const bme280_measurement_t extreme = {.temperature_celsius = -40.0f, .humidity_percent = 100.0f, .pressure_hectopascal = 1100.0f};
    char buffer[320];

    TEST_ASSERT_GREATER_THAN_INT(0, measurements_json_write(buffer, sizeof(buffer), &extreme, 4294967295u));
}

static void test_reports_a_buffer_that_is_too_small(void)
{
    char buffer[64];

    TEST_ASSERT_EQUAL_INT(-1, measurements_json_write(buffer, sizeof(buffer), &MEASUREMENT, 3));
}

static void test_reports_a_buffer_one_byte_short_of_the_terminating_zero(void)
{
    char buffer[sizeof(EXPECTED) - 1];

    TEST_ASSERT_EQUAL_INT(-1, measurements_json_write(buffer, sizeof(buffer), &MEASUREMENT, 3));
}

int main(void)
{
    UNITY_BEGIN();
    RUN_TEST(test_writes_the_agreed_document);
    RUN_TEST(test_returns_the_length_of_the_document);
    RUN_TEST(test_writes_negative_temperatures_with_sign);
    RUN_TEST(test_writes_the_age_in_seconds);
    RUN_TEST(test_fits_into_the_buffer_of_the_server);
    RUN_TEST(test_reports_a_buffer_that_is_too_small);
    RUN_TEST(test_reports_a_buffer_one_byte_short_of_the_terminating_zero);
    return UNITY_END();
}
