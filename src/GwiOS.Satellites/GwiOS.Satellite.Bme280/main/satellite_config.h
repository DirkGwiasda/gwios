#pragma once

#include "esp_err.h"
#include "satellite_config_rules.h"

#ifdef __cplusplus
extern "C" {
#endif

// Configuration of one individual satellite, read from the "config" partition. Everything the firmware
// needs to know about its identity and its surroundings; nothing of it is compiled in.
typedef struct
{
    char name[SATELLITE_NAME_MAX_LENGTH + 1];
    char token[SATELLITE_TOKEN_MAX_LENGTH + 1];
    char wifi_ssid[SATELLITE_WIFI_SSID_MAX_LENGTH + 1];
    char wifi_password[SATELLITE_WIFI_PASSWORD_MAX_LENGTH + 1];
    char gwios_url[SATELLITE_GWIOS_URL_MAX_LENGTH + 1];
} satellite_config_t;

// Reads and checks the whole configuration. Every missing or invalid entry is logged by its key, never by
// its value, because token and Wi-Fi password are secrets. Returns ESP_OK only if all entries are present
// and valid, ESP_ERR_NOT_FOUND if the partition or an entry is missing and ESP_ERR_INVALID_ARG if an entry
// breaks its rule.
esp_err_t satellite_config_load(satellite_config_t *out_config);

#ifdef __cplusplus
}
#endif
