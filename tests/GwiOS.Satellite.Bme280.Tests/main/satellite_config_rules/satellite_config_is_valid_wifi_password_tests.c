#include "satellite_config_rules.h"
#include "unity.h"

void setUp(void)
{
}

void tearDown(void)
{
}

static void test_accepts_an_empty_password_for_an_open_network(void)
{
    TEST_ASSERT_TRUE(satellite_config_is_valid_wifi_password(""));
}

static void test_accepts_eight_characters(void)
{
    TEST_ASSERT_TRUE(satellite_config_is_valid_wifi_password("12345678"));
}

static void test_accepts_sixty_three_characters(void)
{
    TEST_ASSERT_TRUE(satellite_config_is_valid_wifi_password("abcdefghijklmnopqrstuvwxyz0123456789abcdefghijklmnopqrstuvwxyz0"));
}

static void test_accepts_spaces_and_symbols(void)
{
    TEST_ASSERT_TRUE(satellite_config_is_valid_wifi_password("Mein WLAN, 100% sicher!"));
}

static void test_rejects_seven_characters(void)
{
    TEST_ASSERT_FALSE(satellite_config_is_valid_wifi_password("1234567"));
}

static void test_rejects_sixty_four_characters(void)
{
    TEST_ASSERT_FALSE(satellite_config_is_valid_wifi_password("abcdefghijklmnopqrstuvwxyz0123456789abcdefghijklmnopqrstuvwxyz01"));
}

static void test_rejects_characters_outside_printable_ascii(void)
{
    TEST_ASSERT_FALSE(satellite_config_is_valid_wifi_password("gr\xC3\xBCnerbaum"));
    TEST_ASSERT_FALSE(satellite_config_is_valid_wifi_password("password\t1"));
}

int main(void)
{
    UNITY_BEGIN();
    RUN_TEST(test_accepts_an_empty_password_for_an_open_network);
    RUN_TEST(test_accepts_eight_characters);
    RUN_TEST(test_accepts_sixty_three_characters);
    RUN_TEST(test_accepts_spaces_and_symbols);
    RUN_TEST(test_rejects_seven_characters);
    RUN_TEST(test_rejects_sixty_four_characters);
    RUN_TEST(test_rejects_characters_outside_printable_ascii);
    return UNITY_END();
}
