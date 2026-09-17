#pragma once

#include <stdbool.h>

#ifdef __cplusplus
extern "C" {
#endif

// Longest accepted value of each configuration entry, in bytes and without the terminating zero.
#define SATELLITE_NAME_MAX_LENGTH 32
#define SATELLITE_TOKEN_MAX_LENGTH 128
#define SATELLITE_WIFI_SSID_MAX_LENGTH 32
#define SATELLITE_WIFI_PASSWORD_MAX_LENGTH 63
#define SATELLITE_GWIOS_URL_MAX_LENGTH 128

// Shortest accepted token. Shorter tokens could be guessed.
#define SATELLITE_TOKEN_MIN_LENGTH 32

// The name is the id of the satellite and part of the ping URL: 1 to 32 characters out of lowercase
// a-z, digits and '-'.
bool satellite_config_is_valid_name(const char *name);

// The token travels in an HTTP header: 32 to 128 characters out of A-Z, a-z, digits, '-' and '_'
// (the base64url alphabet).
bool satellite_config_is_valid_token(const char *token);

// An SSID has 1 to 32 bytes.
bool satellite_config_is_valid_wifi_ssid(const char *ssid);

// Empty for an open network, otherwise a WPA passphrase of 8 to 63 printable ASCII characters.
bool satellite_config_is_valid_wifi_password(const char *password);

// The base address of GwiOS: starts with "https://", has a host, contains no whitespace, does not end with
// '/' and is at most 128 characters long.
bool satellite_config_is_valid_gwios_url(const char *url);

#ifdef __cplusplus
}
#endif
