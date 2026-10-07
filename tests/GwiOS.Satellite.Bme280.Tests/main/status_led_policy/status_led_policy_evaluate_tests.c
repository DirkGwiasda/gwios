#include "status_led_policy.h"
#include "unity.h"

void setUp(void)
{
}

void tearDown(void)
{
}

// The satellite has been running for an hour: long enough that every window in the policy has passed and
// the test says with its offsets alone how long ago something happened.
#define NOW_MS 3600000

static status_led_state_t evaluate_without_traffic(void)
{
    const status_led_activity_t activity = {.now_ms = NOW_MS};

    return status_led_policy_evaluate(&activity);
}

static status_led_state_t evaluate_after_success(int64_t success_ago_ms)
{
    const status_led_activity_t activity = {
        .now_ms = NOW_MS,
        .has_succeeded = true,
        .last_success_ms = NOW_MS - success_ago_ms,
        .has_requested = true,
        .last_request_ms = NOW_MS - success_ago_ms,
    };

    return status_led_policy_evaluate(&activity);
}

static status_led_state_t evaluate_after_success_and_request(int64_t success_ago_ms, int64_t request_ago_ms)
{
    const status_led_activity_t activity = {
        .now_ms = NOW_MS,
        .has_succeeded = true,
        .last_success_ms = NOW_MS - success_ago_ms,
        .has_requested = true,
        .last_request_ms = NOW_MS - request_ago_ms,
    };

    return status_led_policy_evaluate(&activity);
}

static void test_lights_blue_while_the_firmware_runs(void)
{
    TEST_ASSERT_TRUE(evaluate_without_traffic().blue);
    TEST_ASSERT_TRUE(evaluate_after_success(0).blue);
    TEST_ASSERT_TRUE(evaluate_after_success(STATUS_LED_ONLINE_WINDOW_MS).blue);
}

static void test_reports_a_satellite_without_any_communication_as_offline(void)
{
    const status_led_state_t state = evaluate_without_traffic();

    TEST_ASSERT_FALSE(state.green);
    TEST_ASSERT_TRUE(state.red);
}

static void test_lights_green_after_a_successful_communication(void)
{
    const status_led_state_t state = evaluate_after_success(0);

    TEST_ASSERT_TRUE(state.green);
}

static void test_keeps_green_until_three_minutes_after_the_last_success(void)
{
    TEST_ASSERT_TRUE(evaluate_after_success(STATUS_LED_ONLINE_WINDOW_MS - 1).green);
    TEST_ASSERT_FALSE(evaluate_after_success(STATUS_LED_ONLINE_WINDOW_MS).green);
}

static void test_lights_red_instead_of_green_once_the_communication_is_three_minutes_old(void)
{
    const status_led_state_t state = evaluate_after_success(STATUS_LED_ONLINE_WINDOW_MS);

    TEST_ASSERT_FALSE(state.green);
    TEST_ASSERT_TRUE(state.red);
}

static void test_flashes_red_for_half_a_second_per_request_while_online(void)
{
    TEST_ASSERT_TRUE(evaluate_after_success_and_request(1000, 0).red);
    TEST_ASSERT_TRUE(evaluate_after_success_and_request(1000, STATUS_LED_FLASH_MS - 1).red);
    TEST_ASSERT_FALSE(evaluate_after_success_and_request(1000, STATUS_LED_FLASH_MS).red);
}

static void test_flashes_red_for_a_request_that_did_not_succeed(void)
{
    const status_led_activity_t activity = {
        .now_ms = NOW_MS,
        .has_succeeded = true,
        .last_success_ms = NOW_MS - 60000,
        .has_requested = true,
        .last_request_ms = NOW_MS,
    };

    const status_led_state_t state = status_led_policy_evaluate(&activity);

    TEST_ASSERT_TRUE(state.red);
    TEST_ASSERT_TRUE(state.green);
}

static void test_keeps_a_flashing_satellite_green(void)
{
    const status_led_state_t state = evaluate_after_success_and_request(1000, 0);

    TEST_ASSERT_TRUE(state.green);
}

static void test_does_not_let_requests_alone_count_as_communication(void)
{
    const status_led_activity_t activity = {
        .now_ms = NOW_MS,
        .has_requested = true,
        .last_request_ms = NOW_MS,
    };

    const status_led_state_t state = status_led_policy_evaluate(&activity);

    TEST_ASSERT_FALSE(state.green);
    TEST_ASSERT_TRUE(state.red);
}

static void test_keeps_red_lit_between_the_requests_of_an_offline_satellite(void)
{
    const status_led_activity_t activity = {
        .now_ms = NOW_MS,
        .has_succeeded = true,
        .last_success_ms = NOW_MS - STATUS_LED_ONLINE_WINDOW_MS,
        .has_requested = true,
        .last_request_ms = NOW_MS - STATUS_LED_FLASH_MS,
    };

    TEST_ASSERT_TRUE(status_led_policy_evaluate(&activity).red);
}

int main(void)
{
    UNITY_BEGIN();
    RUN_TEST(test_lights_blue_while_the_firmware_runs);
    RUN_TEST(test_reports_a_satellite_without_any_communication_as_offline);
    RUN_TEST(test_lights_green_after_a_successful_communication);
    RUN_TEST(test_keeps_green_until_three_minutes_after_the_last_success);
    RUN_TEST(test_lights_red_instead_of_green_once_the_communication_is_three_minutes_old);
    RUN_TEST(test_flashes_red_for_half_a_second_per_request_while_online);
    RUN_TEST(test_flashes_red_for_a_request_that_did_not_succeed);
    RUN_TEST(test_keeps_a_flashing_satellite_green);
    RUN_TEST(test_does_not_let_requests_alone_count_as_communication);
    RUN_TEST(test_keeps_red_lit_between_the_requests_of_an_offline_satellite);
    return UNITY_END();
}
