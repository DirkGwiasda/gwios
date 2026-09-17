#include <string.h>

#include "ping_payload.h"
#include "unity.h"

void setUp(void)
{
}

void tearDown(void)
{
}

static ping_payload_t typical_payload(void)
{
    const ping_payload_t payload = {
        .ip_address = {192, 168, 178, 33},
        .board = "ESP32 DevKit C",
        .firmware = "0.1.0",
        .wifi_rssi_dbm = -70,
    };

    return payload;
}

// The exact document agreed with GwiOS in docs/devplan.md.
static const char EXPECTED[] =
    "{\"endpoint\":\"http://192.168.178.33\",\"board\":\"ESP32 DevKit C\",\"firmware\":\"0.1.0\",\"wifiRssiDbm\":-70,"
    "\"sensors\":[{\"type\":\"bme280\",\"quantities\":[\"temperature\",\"humidity\",\"pressure\"]}]}";

static void test_writes_the_agreed_document(void)
{
    const ping_payload_t payload = typical_payload();
    char buffer[512];

    ping_payload_write(buffer, sizeof(buffer), &payload);

    TEST_ASSERT_EQUAL_STRING(EXPECTED, buffer);
}

static void test_returns_the_length_of_the_document(void)
{
    const ping_payload_t payload = typical_payload();
    char buffer[512];

    const int length = ping_payload_write(buffer, sizeof(buffer), &payload);

    TEST_ASSERT_EQUAL_INT((int)strlen(EXPECTED), length);
}

static void test_writes_each_address_byte_without_leading_zeros(void)
{
    ping_payload_t payload = typical_payload();
    const uint8_t address[4] = {10, 0, 0, 255};
    memcpy(payload.ip_address, address, sizeof(address));
    char buffer[512];

    ping_payload_write(buffer, sizeof(buffer), &payload);

    TEST_ASSERT_NOT_NULL(strstr(buffer, "\"endpoint\":\"http://10.0.0.255\","));
}

static void test_fits_into_the_buffer_of_the_ping_client(void)
{
    // Longest address, weakest signal. ping_client.c uses 320 bytes.
    ping_payload_t payload = typical_payload();
    const uint8_t address[4] = {255, 255, 255, 255};
    memcpy(payload.ip_address, address, sizeof(address));
    payload.wifi_rssi_dbm = -128;
    char buffer[320];

    TEST_ASSERT_GREATER_THAN_INT(0, ping_payload_write(buffer, sizeof(buffer), &payload));
}

static void test_rejects_texts_that_would_break_the_json(void)
{
    ping_payload_t payload = typical_payload();
    char buffer[512];

    payload.board = "ESP32 \"DevKit\"";
    TEST_ASSERT_EQUAL_INT(-1, ping_payload_write(buffer, sizeof(buffer), &payload));

    payload.board = "ESP32 DevKit C";
    payload.firmware = "0.1.0\\beta";
    TEST_ASSERT_EQUAL_INT(-1, ping_payload_write(buffer, sizeof(buffer), &payload));

    payload.firmware = "0.1.0\n";
    TEST_ASSERT_EQUAL_INT(-1, ping_payload_write(buffer, sizeof(buffer), &payload));
}

static void test_reports_a_buffer_one_byte_short_of_the_terminating_zero(void)
{
    const ping_payload_t payload = typical_payload();
    char buffer[sizeof(EXPECTED) - 1];

    TEST_ASSERT_EQUAL_INT(-1, ping_payload_write(buffer, sizeof(buffer), &payload));
}

int main(void)
{
    UNITY_BEGIN();
    RUN_TEST(test_writes_the_agreed_document);
    RUN_TEST(test_returns_the_length_of_the_document);
    RUN_TEST(test_writes_each_address_byte_without_leading_zeros);
    RUN_TEST(test_fits_into_the_buffer_of_the_ping_client);
    RUN_TEST(test_rejects_texts_that_would_break_the_json);
    RUN_TEST(test_reports_a_buffer_one_byte_short_of_the_terminating_zero);
    return UNITY_END();
}
