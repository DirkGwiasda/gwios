#pragma once

#include <stdbool.h>
#include <stdint.h>

#include "esp_err.h"
#include "esp_netif_ip_addr.h"

#ifdef __cplusplus
extern "C" {
#endif

// Connects the satellite to the Wi-Fi network as a station and keeps it connected: after a loss it
// reconnects on its own with increasing pauses (see wifi_reconnect_policy.h). Returns once the driver is
// started; the connection itself is established in the background. The hostname is how the satellite
// appears in the router's device list. Call once.
esp_err_t wifi_connection_start(const char *hostname, const char *ssid, const char *password);

// Tells whether the satellite currently has an IP address.
bool wifi_connection_is_connected(void);

// Returns the current IPv4 address, or ESP_ERR_INVALID_STATE while not connected.
esp_err_t wifi_connection_get_ip_address(esp_ip4_addr_t *out_address);

// Returns the signal strength of the access point in dBm, or ESP_ERR_INVALID_STATE while not connected.
esp_err_t wifi_connection_get_rssi(int8_t *out_rssi_dbm);

#ifdef __cplusplus
}
#endif
