#include "status_led_policy.h"

status_led_state_t status_led_policy_evaluate(const status_led_activity_t *activity)
{
    const bool online =
        activity->has_succeeded && activity->now_ms - activity->last_success_ms < STATUS_LED_ONLINE_WINDOW_MS;

    const bool flashing =
        activity->has_requested && activity->now_ms - activity->last_request_ms < STATUS_LED_FLASH_MS;

    const status_led_state_t state = {
        .blue = true,
        .green = online,
        // Being offline outranks the flash: a satellite that cannot reach GwiOS keeps talking to itself, and
        // a lamp blinking away on its own request traffic would hide that.
        .red = online ? flashing : true,
    };

    return state;
}
