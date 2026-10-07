#pragma once

#include <stdbool.h>

#include "driver/gpio.h"
#include "esp_err.h"

#ifdef __cplusplus
extern "C" {
#endif

// Drives the three status lamps according to status_led_policy.h: blue burns while the firmware runs, green
// while GwiOS was reached within the last three minutes, red as the warning when it was not and as a short
// flash per request when it was. The pins are wired against ground, so a lamp lights on a high level.
// Starts the task that keeps them up to date. Call once, before anything that reports traffic.
esp_err_t status_led_start(gpio_num_t blue, gpio_num_t green, gpio_num_t red);

// Reports one request that went out to GwiOS or came in from it, no matter how it ended; succeeded tells
// whether the two really talked to each other (a ping GwiOS accepted, a pull with a valid token). Called
// from the ping and server tasks, which is why the timestamps are kept under a lock.
void status_led_note_request(bool succeeded);

#ifdef __cplusplus
}
#endif
