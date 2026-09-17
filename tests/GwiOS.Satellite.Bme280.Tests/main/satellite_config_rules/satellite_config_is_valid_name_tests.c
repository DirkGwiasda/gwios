#include "satellite_config_rules.h"
#include "unity.h"

void setUp(void)
{
}

void tearDown(void)
{
}

static void test_accepts_lowercase_letters_digits_and_hyphens(void)
{
    TEST_ASSERT_TRUE(satellite_config_is_valid_name("keller-2"));
}

static void test_accepts_a_single_character(void)
{
    TEST_ASSERT_TRUE(satellite_config_is_valid_name("a"));
}

static void test_accepts_thirty_two_characters(void)
{
    TEST_ASSERT_TRUE(satellite_config_is_valid_name("abcdefghijklmnopqrstuvwxyz012345"));
}

static void test_rejects_an_empty_name(void)
{
    TEST_ASSERT_FALSE(satellite_config_is_valid_name(""));
}

static void test_rejects_thirty_three_characters(void)
{
    TEST_ASSERT_FALSE(satellite_config_is_valid_name("abcdefghijklmnopqrstuvwxyz0123456"));
}

static void test_rejects_uppercase_letters(void)
{
    TEST_ASSERT_FALSE(satellite_config_is_valid_name("Wohnzimmer"));
}

static void test_rejects_umlauts(void)
{
    TEST_ASSERT_FALSE(satellite_config_is_valid_name("k\xC3\xBC" "che"));
}

static void test_rejects_characters_that_change_the_url_path(void)
{
    TEST_ASSERT_FALSE(satellite_config_is_valid_name("keller/2"));
    TEST_ASSERT_FALSE(satellite_config_is_valid_name("keller 2"));
    TEST_ASSERT_FALSE(satellite_config_is_valid_name("keller_2"));
}

int main(void)
{
    UNITY_BEGIN();
    RUN_TEST(test_accepts_lowercase_letters_digits_and_hyphens);
    RUN_TEST(test_accepts_a_single_character);
    RUN_TEST(test_accepts_thirty_two_characters);
    RUN_TEST(test_rejects_an_empty_name);
    RUN_TEST(test_rejects_thirty_three_characters);
    RUN_TEST(test_rejects_uppercase_letters);
    RUN_TEST(test_rejects_umlauts);
    RUN_TEST(test_rejects_characters_that_change_the_url_path);
    return UNITY_END();
}
