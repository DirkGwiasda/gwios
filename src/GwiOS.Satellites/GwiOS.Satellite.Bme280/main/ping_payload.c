#include "ping_payload.h"

#include <stdbool.h>
#include <stdio.h>

// The sensors of this firmware are fixed, so their description is too.
#define PING_FORMAT                                                                                         \
    "{\"endpoint\":\"http://%u.%u.%u.%u\",\"board\":\"%s\",\"firmware\":\"%s\",\"wifiRssiDbm\":%d,"         \
    "\"sensors\":[{\"type\":\"bme280\",\"quantities\":[\"temperature\",\"humidity\",\"pressure\"]}]}"

// The texts come from constants of the firmware, so escaping them would only hide a mistake. Rejecting
// them makes the mistake visible instead of producing broken JSON.
static bool is_json_safe(const char *text)
{
    for (const char *character = text; *character != '\0'; character++)
    {
        if (*character == '"' || *character == '\\' || (unsigned char)*character < 0x20)
        {
            return false;
        }
    }

    return true;
}

int ping_payload_write(char *buffer, size_t buffer_size, const ping_payload_t *payload)
{
    if (!is_json_safe(payload->board) || !is_json_safe(payload->firmware))
    {
        return -1;
    }

    const int length = snprintf(buffer,
                                buffer_size,
                                PING_FORMAT,
                                payload->ip_address[0],
                                payload->ip_address[1],
                                payload->ip_address[2],
                                payload->ip_address[3],
                                payload->board,
                                payload->firmware,
                                payload->wifi_rssi_dbm);

    if (length < 0 || (size_t)length >= buffer_size)
    {
        return -1;
    }

    return length;
}
