#include "wifi_reconnect_policy.h"

uint32_t wifi_reconnect_delay_ms(uint32_t failed_attempts)
{
    if (failed_attempts == 0)
    {
        return 0;
    }

    // Doubling in a loop instead of shifting by failed_attempts: the shift would overflow after 32 failures,
    // the loop stops as soon as the limit is reached.
    uint32_t delay = WIFI_RECONNECT_FIRST_DELAY_MS;
    for (uint32_t attempt = 1; attempt < failed_attempts && delay < WIFI_RECONNECT_MAX_DELAY_MS; attempt++)
    {
        delay *= 2;
    }

    return delay < WIFI_RECONNECT_MAX_DELAY_MS ? delay : WIFI_RECONNECT_MAX_DELAY_MS;
}
