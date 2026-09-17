#pragma once

#include "esp_err.h"

#ifdef __cplusplus
extern "C" {
#endif

// Starts the HTTP server on port 80 that GwiOS pulls measurements from: GET /measurements answers with the
// latest measurement from measurement_store.h as JSON (see measurements_json.h). Requests without
// "Authorization: Bearer <token>" get 401, requests before the first measurement 503. The token must stay
// valid for the lifetime of the server. Call once.
esp_err_t measurement_server_start(const char *token);

#ifdef __cplusplus
}
#endif
