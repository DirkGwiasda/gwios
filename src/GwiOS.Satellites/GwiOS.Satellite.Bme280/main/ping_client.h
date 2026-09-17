#pragma once

#include "esp_err.h"
#include "satellite_config.h"

#ifdef __cplusplus
extern "C" {
#endif

#define PING_INTERVAL_MS 60000

// Starts a task that sends a ping to GwiOS over HTTPS every 60 seconds while Wi-Fi is connected, the first one
// as soon as the connection is up. The server certificate is checked against the CA bundle of ESP-IDF.
// Failed pings are logged and simply repeated with the next one. Configuration and board name must stay
// valid for the lifetime of the firmware. Call once.
esp_err_t ping_client_start(const satellite_config_t *config, const char *board);

#ifdef __cplusplus
}
#endif
