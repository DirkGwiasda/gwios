#pragma once

#include <stdbool.h>
#include <stdint.h>

#include "bme280_measurement.h"
#include "esp_err.h"

#ifdef __cplusplus
extern "C" {
#endif

// Keeps the most recent successful measurement for the HTTP server. The measuring loop writes, the server
// task reads; both may do so at any time. Call measurement_store_init once before anything else.
esp_err_t measurement_store_init(void);

// Replaces the stored measurement and remembers when it was taken.
void measurement_store_put(const bme280_measurement_t *measurement);

// Copies the stored measurement and returns its age in whole seconds. Returns false while no measurement
// has been stored yet. A failing sensor does not clear the store: the age keeps growing instead, which is
// how GwiOS can tell a stale value.
bool measurement_store_get(bme280_measurement_t *out_measurement, uint32_t *out_age_seconds);

#ifdef __cplusplus
}
#endif
