#include "measurements_json.h"

#include <stdio.h>

// Every string in the document is a fixed identifier, so the body is formatted directly instead of pulling
// in a JSON library. The unit of temperature is UTF-8, which JSON allows unescaped.
#define MEASUREMENTS_FORMAT                                                                    \
    "{\"measurements\":["                                                                      \
    "{\"sensor\":\"bme280\",\"quantity\":\"temperature\",\"value\":%.2f,\"unit\":\"\xC2\xB0" "C\"}," \
    "{\"sensor\":\"bme280\",\"quantity\":\"humidity\",\"value\":%.2f,\"unit\":\"%%\"},"        \
    "{\"sensor\":\"bme280\",\"quantity\":\"pressure\",\"value\":%.2f,\"unit\":\"hPa\"}"        \
    "],\"ageSeconds\":%u}"

int measurements_json_write(char *buffer, size_t buffer_size, const bme280_measurement_t *measurement, uint32_t age_seconds)
{
    const int length = snprintf(buffer,
                                buffer_size,
                                MEASUREMENTS_FORMAT,
                                (double)measurement->temperature_celsius,
                                (double)measurement->humidity_percent,
                                (double)measurement->pressure_hectopascal,
                                (unsigned)age_seconds);

    if (length < 0 || (size_t)length >= buffer_size)
    {
        return -1;
    }

    return length;
}
