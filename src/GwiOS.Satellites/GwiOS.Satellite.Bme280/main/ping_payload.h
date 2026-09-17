#pragma once

#include <stddef.h>
#include <stdint.h>

#ifdef __cplusplus
extern "C" {
#endif

// Everything a ping reports about the satellite apart from its name, which travels in the URL.
typedef struct
{
    // Own IPv4 address, most significant byte first (192, 168, 178, 33). GwiOS pulls measurements from it.
    uint8_t ip_address[4];

    // Readable type name of the board, such as "ESP32 DevKit C".
    const char *board;

    // Firmware version, such as "0.1.0".
    const char *firmware;

    // Current signal strength of the Wi-Fi access point.
    int8_t wifi_rssi_dbm;
} ping_payload_t;

// Writes the request body of the ping as agreed with GwiOS (see docs/devplan.md):
// {"endpoint":"http://192.168.178.33","board":"ESP32 DevKit C","firmware":"0.1.0","wifiRssiDbm":-70,
//  "sensors":[{"type":"bme280","quantities":["temperature","humidity","pressure"]}]}
// Board and firmware are written as they are and must not contain '"', '\' or control characters. Returns
// the length written without the terminating zero, or -1 if the buffer is too small or a text would need
// escaping, in which case the buffer content must not be used.
int ping_payload_write(char *buffer, size_t buffer_size, const ping_payload_t *payload);

#ifdef __cplusplus
}
#endif
