#include "unity.h"
#include "wifi_reconnect_policy.h"

void setUp(void)
{
}

void tearDown(void)
{
}

static void test_does_not_wait_without_failures(void)
{
    TEST_ASSERT_EQUAL_UINT32(0, wifi_reconnect_delay_ms(0));
}

static void test_waits_one_second_after_the_first_failure(void)
{
    TEST_ASSERT_EQUAL_UINT32(1000, wifi_reconnect_delay_ms(1));
}

static void test_doubles_the_wait_with_each_further_failure(void)
{
    TEST_ASSERT_EQUAL_UINT32(2000, wifi_reconnect_delay_ms(2));
    TEST_ASSERT_EQUAL_UINT32(4000, wifi_reconnect_delay_ms(3));
    TEST_ASSERT_EQUAL_UINT32(32000, wifi_reconnect_delay_ms(6));
}

static void test_waits_at_most_sixty_seconds(void)
{
    TEST_ASSERT_EQUAL_UINT32(60000, wifi_reconnect_delay_ms(7));
    TEST_ASSERT_EQUAL_UINT32(60000, wifi_reconnect_delay_ms(100));
}

static void test_does_not_overflow_after_many_failures(void)
{
    TEST_ASSERT_EQUAL_UINT32(60000, wifi_reconnect_delay_ms(UINT32_MAX));
}

int main(void)
{
    UNITY_BEGIN();
    RUN_TEST(test_does_not_wait_without_failures);
    RUN_TEST(test_waits_one_second_after_the_first_failure);
    RUN_TEST(test_doubles_the_wait_with_each_further_failure);
    RUN_TEST(test_waits_at_most_sixty_seconds);
    RUN_TEST(test_does_not_overflow_after_many_failures);
    return UNITY_END();
}
