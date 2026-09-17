#include <string.h>

#include "satellite_config_rules.h"
#include "unity.h"

void setUp(void)
{
}

void tearDown(void)
{
}

static void test_accepts_the_gwios_address(void)
{
    TEST_ASSERT_TRUE(satellite_config_is_valid_gwios_url("https://webui.gwios.gwiasda.net"));
}

static void test_accepts_a_port_and_a_path_prefix(void)
{
    TEST_ASSERT_TRUE(satellite_config_is_valid_gwios_url("https://gwios.local:8443/portal"));
}

static void test_accepts_one_hundred_twenty_eight_characters(void)
{
    char url[SATELLITE_GWIOS_URL_MAX_LENGTH + 2] = "https://";
    memset(url + 8, 'x', SATELLITE_GWIOS_URL_MAX_LENGTH - 8);
    url[SATELLITE_GWIOS_URL_MAX_LENGTH] = '\0';

    TEST_ASSERT_TRUE(satellite_config_is_valid_gwios_url(url));
}

static void test_rejects_one_hundred_twenty_nine_characters(void)
{
    char url[SATELLITE_GWIOS_URL_MAX_LENGTH + 2] = "https://";
    memset(url + 8, 'x', SATELLITE_GWIOS_URL_MAX_LENGTH - 7);
    url[SATELLITE_GWIOS_URL_MAX_LENGTH + 1] = '\0';

    TEST_ASSERT_FALSE(satellite_config_is_valid_gwios_url(url));
}

static void test_rejects_plain_http_because_the_token_would_travel_unencrypted(void)
{
    TEST_ASSERT_FALSE(satellite_config_is_valid_gwios_url("http://webui.gwios.gwiasda.net"));
}

static void test_rejects_a_url_without_host(void)
{
    TEST_ASSERT_FALSE(satellite_config_is_valid_gwios_url("https://"));
    TEST_ASSERT_FALSE(satellite_config_is_valid_gwios_url(""));
}

static void test_rejects_a_trailing_slash(void)
{
    TEST_ASSERT_FALSE(satellite_config_is_valid_gwios_url("https://webui.gwios.gwiasda.net/"));
}

static void test_rejects_whitespace(void)
{
    TEST_ASSERT_FALSE(satellite_config_is_valid_gwios_url("https://webui.gwios.gwiasda.net "));
    TEST_ASSERT_FALSE(satellite_config_is_valid_gwios_url("https://webui gwios"));
}

int main(void)
{
    UNITY_BEGIN();
    RUN_TEST(test_accepts_the_gwios_address);
    RUN_TEST(test_accepts_a_port_and_a_path_prefix);
    RUN_TEST(test_accepts_one_hundred_twenty_eight_characters);
    RUN_TEST(test_rejects_one_hundred_twenty_nine_characters);
    RUN_TEST(test_rejects_plain_http_because_the_token_would_travel_unencrypted);
    RUN_TEST(test_rejects_a_url_without_host);
    RUN_TEST(test_rejects_a_trailing_slash);
    RUN_TEST(test_rejects_whitespace);
    return UNITY_END();
}
