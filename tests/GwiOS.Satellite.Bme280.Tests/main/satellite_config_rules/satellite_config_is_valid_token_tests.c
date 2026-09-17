#include <string.h>

#include "satellite_config_rules.h"
#include "unity.h"

void setUp(void)
{
}

void tearDown(void)
{
}

static void fill(char *buffer, char character, size_t length)
{
    memset(buffer, character, length);
    buffer[length] = '\0';
}

static void test_accepts_the_base64url_alphabet(void)
{
    TEST_ASSERT_TRUE(satellite_config_is_valid_token("AZaz09-_AZaz09-_AZaz09-_AZaz09-_"));
}

static void test_accepts_thirty_two_characters(void)
{
    char token[SATELLITE_TOKEN_MAX_LENGTH + 2];
    fill(token, 'x', SATELLITE_TOKEN_MIN_LENGTH);

    TEST_ASSERT_TRUE(satellite_config_is_valid_token(token));
}

static void test_accepts_one_hundred_twenty_eight_characters(void)
{
    char token[SATELLITE_TOKEN_MAX_LENGTH + 2];
    fill(token, 'x', SATELLITE_TOKEN_MAX_LENGTH);

    TEST_ASSERT_TRUE(satellite_config_is_valid_token(token));
}

static void test_rejects_thirty_one_characters(void)
{
    char token[SATELLITE_TOKEN_MAX_LENGTH + 2];
    fill(token, 'x', SATELLITE_TOKEN_MIN_LENGTH - 1);

    TEST_ASSERT_FALSE(satellite_config_is_valid_token(token));
}

static void test_rejects_one_hundred_twenty_nine_characters(void)
{
    char token[SATELLITE_TOKEN_MAX_LENGTH + 2];
    fill(token, 'x', SATELLITE_TOKEN_MAX_LENGTH + 1);

    TEST_ASSERT_FALSE(satellite_config_is_valid_token(token));
}

static void test_rejects_characters_that_break_an_http_header(void)
{
    TEST_ASSERT_FALSE(satellite_config_is_valid_token("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx "));
    TEST_ASSERT_FALSE(satellite_config_is_valid_token("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\r"));
    TEST_ASSERT_FALSE(satellite_config_is_valid_token("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx:"));
}

static void test_rejects_the_padding_and_symbols_of_plain_base64(void)
{
    TEST_ASSERT_FALSE(satellite_config_is_valid_token("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx="));
    TEST_ASSERT_FALSE(satellite_config_is_valid_token("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx+"));
    TEST_ASSERT_FALSE(satellite_config_is_valid_token("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx/"));
}

int main(void)
{
    UNITY_BEGIN();
    RUN_TEST(test_accepts_the_base64url_alphabet);
    RUN_TEST(test_accepts_thirty_two_characters);
    RUN_TEST(test_accepts_one_hundred_twenty_eight_characters);
    RUN_TEST(test_rejects_thirty_one_characters);
    RUN_TEST(test_rejects_one_hundred_twenty_nine_characters);
    RUN_TEST(test_rejects_characters_that_break_an_http_header);
    RUN_TEST(test_rejects_the_padding_and_symbols_of_plain_base64);
    return UNITY_END();
}
