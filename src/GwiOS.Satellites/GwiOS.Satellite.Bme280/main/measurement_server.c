#include "measurement_server.h"

#include "bearer_token.h"
#include "esp_http_server.h"
#include "esp_log.h"
#include "measurement_store.h"
#include "measurements_json.h"
#include "satellite_config_rules.h"
#include "status_led.h"

#define AUTHORIZATION_HEADER "Authorization"

// "Bearer " plus the longest valid token plus the terminating zero. Anything longer cannot be valid and is
// rejected without being read.
#define AUTHORIZATION_BUFFER_SIZE (sizeof("Bearer ") + SATELLITE_TOKEN_MAX_LENGTH)

#define RESPONSE_BUFFER_SIZE 320

// Formatting floats with snprintf needs more stack than the 4 KB the server task gets by default.
#define SERVER_STACK_SIZE 6144

static const char *TAG = "measurement_server";

static const char *expected_token = NULL;

static bool is_authorized(httpd_req_t *request)
{
    const size_t length = httpd_req_get_hdr_value_len(request, AUTHORIZATION_HEADER);
    if (length == 0 || length >= AUTHORIZATION_BUFFER_SIZE)
    {
        return false;
    }

    char authorization[AUTHORIZATION_BUFFER_SIZE];
    if (httpd_req_get_hdr_value_str(request, AUTHORIZATION_HEADER, authorization, sizeof(authorization)) != ESP_OK)
    {
        return false;
    }

    return bearer_token_matches(authorization, expected_token);
}

static esp_err_t reject_unauthorized(httpd_req_t *request)
{
    ESP_LOGW(TAG, "Rejected request without valid token.");
    httpd_resp_set_hdr(request, "WWW-Authenticate", "Bearer");

    return httpd_resp_send_err(request, HTTPD_401_UNAUTHORIZED, "Missing or invalid token");
}

static esp_err_t send_measurements(httpd_req_t *request)
{
    bme280_measurement_t measurement;
    uint32_t age_seconds = 0;
    if (!measurement_store_get(&measurement, &age_seconds))
    {
        httpd_resp_set_status(request, "503 Service Unavailable");
        httpd_resp_set_type(request, "text/plain");

        return httpd_resp_sendstr(request, "No measurement yet");
    }

    char body[RESPONSE_BUFFER_SIZE];
    const int length = measurements_json_write(body, sizeof(body), &measurement, age_seconds);
    if (length < 0)
    {
        ESP_LOGE(TAG, "Response buffer too small.");

        return httpd_resp_send_500(request);
    }

    httpd_resp_set_type(request, "application/json; charset=utf-8");

    return httpd_resp_send(request, body, length);
}

static esp_err_t handle_get_measurements(httpd_req_t *request)
{
    const bool authorized = is_authorized(request);

    // A request with a valid token comes from GwiOS, so it proves the connection works — even when there is
    // no measurement to hand out yet. One without stays mere traffic: anyone in the network can knock.
    status_led_note_request(authorized);

    if (!authorized)
    {
        return reject_unauthorized(request);
    }

    return send_measurements(request);
}

esp_err_t measurement_server_start(const char *token)
{
    expected_token = token;

    httpd_config_t config = HTTPD_DEFAULT_CONFIG();
    config.stack_size = SERVER_STACK_SIZE;

    httpd_handle_t server = NULL;
    esp_err_t result = httpd_start(&server, &config);
    if (result != ESP_OK)
    {
        return result;
    }

    const httpd_uri_t measurements = {
        .uri = "/measurements",
        .method = HTTP_GET,
        .handler = handle_get_measurements,
    };

    result = httpd_register_uri_handler(server, &measurements);
    if (result != ESP_OK)
    {
        httpd_stop(server);

        return result;
    }

    ESP_LOGI(TAG, "Serving GET /measurements on port %u.", (unsigned)config.server_port);

    return ESP_OK;
}
