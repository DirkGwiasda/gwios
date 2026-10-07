#pragma once

#include <stdbool.h>
#include <stdint.h>

#ifdef __cplusplus
extern "C" {
#endif

// A satellite counts as online while it talked to GwiOS successfully within the last three minutes. The
// window matches the one GwiOS uses to call a satellite offline (three missed pings), so both sides agree.
#define STATUS_LED_ONLINE_WINDOW_MS 180000

// How long the red lamp reports a single request. Long enough to be seen, short enough that the pings every
// 60 s and the pulls every 30 s stay separate flashes.
#define STATUS_LED_FLASH_MS 500

// What the satellite knows about its traffic with GwiOS at a given moment. The "has_" flags tell a fresh
// satellite (nothing happened yet) apart from one whose last event lies at time zero.
typedef struct
{
    int64_t now_ms;
    bool has_succeeded;
    int64_t last_success_ms;
    bool has_requested;
    int64_t last_request_ms;
} status_led_activity_t;

// Which of the three lamps are lit.
typedef struct
{
    bool blue;
    bool green;
    bool red;
} status_led_state_t;

// Blue is lit whenever the firmware runs. Green reports that GwiOS was reached within the last three
// minutes. Red is the standing warning while that is not the case; while the satellite is online the same
// lamp flashes for 500 ms per request instead, so one lamp shows both the trouble and the traffic.
status_led_state_t status_led_policy_evaluate(const status_led_activity_t *activity);

#ifdef __cplusplus
}
#endif
