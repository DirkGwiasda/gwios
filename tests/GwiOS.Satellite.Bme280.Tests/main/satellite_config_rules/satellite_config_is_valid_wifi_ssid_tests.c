#include "satellite_config_rules.h"
#include "unity.h"

void setUp(void)
{
}

void tearDown(void)
{
}

static void test_accepts_a_typical_ssid(void)
{
    TEST_ASSERT_TRUE(satellite_config_is_valid_wifi_ssid("FRITZ!Box 7590 XY"));
}

static void test_accepts_thirty_two_bytes(void)
{
    TEST_ASSERT_TRUE(satellite_config_is_valid_wifi_ssid("abcdefghijklmnopqrstuvwxyz012345"));
}

static void test_accepts_umlauts_because_an_ssid_is_arbitrary_bytes(void)
{
    TEST_ASSERT_TRUE(satellite_config_is_valid_wifi_ssid("Gw\xC3\xA4" "stenetz"));
}

static void test_rejects_an_empty_ssid(void)
{
    TEST_ASSERT_FALSE(satellite_config_is_valid_wifi_ssid(""));
}

static void test_rejects_thirty_three_bytes(void)
{
    TEST_ASSERT_FALSE(satellite_config_is_valid_wifi_ssid("abcdefghijklmnopqrstuvwxyz0123456"));
}

int main(void)
{
    UNITY_BEGIN();
    RUN_TEST(test_accepts_a_typical_ssid);
    RUN_TEST(test_accepts_thirty_two_bytes);
    RUN_TEST(test_accepts_umlauts_because_an_ssid_is_arbitrary_bytes);
    RUN_TEST(test_rejects_an_empty_ssid);
    RUN_TEST(test_rejects_thirty_three_bytes);
    return UNITY_END();
}
