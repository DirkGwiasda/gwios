#pragma once

#include <stddef.h>
#include <stdint.h>

#include "bme280_measurement.h"

#ifdef __cplusplus
extern "C" {
#endif

// Writes the response body of GET /measurements as agreed with GwiOS (see docs/devplan.md):
// {"measurements":[{"sensor":"bme280","quantity":"temperature","value":24.20,"unit":"°C"},...],"ageSeconds":3}
// Values have two decimals. Returns the length written without the terminating zero, or -1 if the buffer is
// too small, in which case its content must not be used.
int measurements_json_write(char *buffer, size_t buffer_size, const bme280_measurement_t *measurement, uint32_t age_seconds);

#ifdef __cplusplus
}
#endif
