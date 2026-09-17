#include <string.h>

#include "ping_url.h"
#include "unity.h"

void setUp(void)
{
}

void tearDown(void)
{
}

static void test_appends_the_endpoint_path_with_the_name(void)
{
    char buffer[256];

    ping_url_write(buffer, sizeof(buffer), "https://webui.gwios.gwiasda.net", "arbeitszimmer");

    TEST_ASSERT_EQUAL_STRING("https://webui.gwios.gwiasda.net/api/satellites/arbeitszimmer/ping", buffer);
}

static void test_keeps_a_path_prefix_of_the_gwios_url(void)
{
    char buffer[256];

    ping_url_write(buffer, sizeof(buffer), "https://gwios.local:8443/portal", "keller-2");

    TEST_ASSERT_EQUAL_STRING("https://gwios.local:8443/portal/api/satellites/keller-2/ping", buffer);
}

static void test_returns_the_length_of_the_url(void)
{
    char buffer[256];

    const int length = ping_url_write(buffer, sizeof(buffer), "https://a.b", "c");

    TEST_ASSERT_EQUAL_INT((int)strlen("https://a.b/api/satellites/c/ping"), length);
}

static void test_reports_a_buffer_one_byte_short_of_the_terminating_zero(void)
{
    char buffer[sizeof("https://a.b/api/satellites/c/ping") - 1];

    TEST_ASSERT_EQUAL_INT(-1, ping_url_write(buffer, sizeof(buffer), "https://a.b", "c"));
}

int main(void)
{
    UNITY_BEGIN();
    RUN_TEST(test_appends_the_endpoint_path_with_the_name);
    RUN_TEST(test_keeps_a_path_prefix_of_the_gwios_url);
    RUN_TEST(test_returns_the_length_of_the_url);
    RUN_TEST(test_reports_a_buffer_one_byte_short_of_the_terminating_zero);
    return UNITY_END();
}
