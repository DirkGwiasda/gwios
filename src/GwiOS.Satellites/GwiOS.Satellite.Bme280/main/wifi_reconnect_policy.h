#pragma once

#include <stdint.h>

#ifdef __cplusplus
extern "C" {
#endif

#define WIFI_RECONNECT_FIRST_DELAY_MS 1000
#define WIFI_RECONNECT_MAX_DELAY_MS 60000

// Returns how long to wait before the next connection attempt after the given number of consecutive
// failures: 1 s after the first, doubling with each further failure, at most 60 s. Zero failures need no
// wait. Backing off keeps a satellite from flooding an access point that is down or refusing it.
uint32_t wifi_reconnect_delay_ms(uint32_t failed_attempts);

#ifdef __cplusplus
}
#endif
