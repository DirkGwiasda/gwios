#include "ping_client.h"

#include <stdio.h>

#include "esp_app_desc.h"
#include "esp_crt_bundle.h"
#include "esp_http_client.h"
#include "esp_log.h"
#include "freertos/FreeRTOS.h"
#include "freertos/task.h"
#include "ping_payload.h"
#include "ping_url.h"
#include "status_led.h"
#include "wifi_connection.h"

// The TLS handshake needs far more stack than an ordinary task.
#define PING_TASK_STACK_SIZE 8192
#define PING_TASK_PRIORITY 5

#define CONNECTION_CHECK_INTERVAL_MS 1000
#define REQUEST_TIMEOUT_MS 10000

#define URL_BUFFER_SIZE (SATELLITE_GWIOS_URL_MAX_LENGTH + sizeof("/api/satellites//ping") + SATELLITE_NAME_MAX_LENGTH)
#define PAYLOAD_BUFFER_SIZE 320
#define AUTHORIZATION_BUFFER_SIZE (sizeof("Bearer ") + SATELLITE_TOKEN_MAX_LENGTH)

static const char *TAG = "ping_client";

typedef struct
{
    const satellite_config_t *config;
    const char *board;
} ping_context_t;

static esp_err_t collect_payload(const ping_context_t *context, ping_payload_t *out_payload)
{
    esp_ip4_addr_t address;
    esp_err_t result = wifi_connection_get_ip_address(&address);
    if (result != ESP_OK)
    {
        return result;
    }

    int8_t rssi_dbm = 0;
    result = wifi_connection_get_rssi(&rssi_dbm);
    if (result != ESP_OK)
    {
        return result;
    }

    out_payload->ip_address[0] = esp_ip4_addr1(&address);
    out_payload->ip_address[1] = esp_ip4_addr2(&address);
    out_payload->ip_address[2] = esp_ip4_addr3(&address);
    out_payload->ip_address[3] = esp_ip4_addr4(&address);
    out_payload->board = context->board;
    out_payload->firmware = esp_app_get_description()->version;
    out_payload->wifi_rssi_dbm = rssi_dbm;

    return ESP_OK;
}

static void log_response(int status, int8_t rssi_dbm)
{
    if (status == 204)
    {
        ESP_LOGI(TAG, "Ping accepted (signal %d dBm).", rssi_dbm);
    }
    else if (status == 401)
    {
        ESP_LOGW(TAG, "Ping rejected: GwiOS does not accept the token (401).");
    }
    else if (status == 404)
    {
        ESP_LOGW(TAG, "Ping rejected: GwiOS does not know this satellite (404).");
    }
    else if (status >= 300 && status < 400)
    {
        ESP_LOGW(TAG, "Ping redirected (%d) and not followed. Is the ping endpoint missing in GwiOS?", status);
    }
    else
    {
        ESP_LOGW(TAG, "Ping answered with unexpected status %d.", status);
    }
}

static esp_err_t post(const char *url, const char *token, const char *body, int body_length, int *out_status)
{
    const esp_http_client_config_t client_config = {
        .url = url,
        .method = HTTP_METHOD_POST,
        .timeout_ms = REQUEST_TIMEOUT_MS,
        .crt_bundle_attach = esp_crt_bundle_attach,
        // Following a redirect would hand the token to whatever server it points to, for example a login page.
        .disable_auto_redirect = true,
    };

    esp_http_client_handle_t client = esp_http_client_init(&client_config);
    if (client == NULL)
    {
        return ESP_ERR_NO_MEM;
    }

    char authorization[AUTHORIZATION_BUFFER_SIZE];
    snprintf(authorization, sizeof(authorization), "Bearer %s", token);

    esp_http_client_set_header(client, "Authorization", authorization);
    esp_http_client_set_header(client, "Content-Type", "application/json");
    esp_http_client_set_post_field(client, body, body_length);

    esp_err_t result = esp_http_client_perform(client);
    if (result == ESP_OK)
    {
        *out_status = esp_http_client_get_status_code(client);
    }

    esp_http_client_cleanup(client);

    return result;
}

static void send_ping(const ping_context_t *context)
{
    ping_payload_t payload;
    if (collect_payload(context, &payload) != ESP_OK)
    {
        // The connection dropped between the check and now; the next round tries again.
        return;
    }

    char url[URL_BUFFER_SIZE];
    char body[PAYLOAD_BUFFER_SIZE];
    const int url_length = ping_url_write(url, sizeof(url), context->config->gwios_url, context->config->name);
    const int body_length = ping_payload_write(body, sizeof(body), &payload);
    if (url_length < 0 || body_length < 0)
    {
        ESP_LOGE(TAG, "Ping could not be composed (URL %d, body %d).", url_length, body_length);

        return;
    }

    int status = 0;
    esp_err_t result = post(url, context->config->token, body, body_length, &status);

    // The attempt itself counts as traffic, whatever came of it; only an answer GwiOS accepted proves that
    // the two really talked to each other.
    status_led_note_request(result == ESP_OK && status >= 200 && status < 300);

    if (result != ESP_OK)
    {
        ESP_LOGW(TAG, "Ping to %s failed: %s.", url, esp_err_to_name(result));

        return;
    }

    log_response(status, payload.wifi_rssi_dbm);
}

static void run_ping_loop(void *argument)
{
    const ping_context_t *context = argument;

    while (true)
    {
        if (!wifi_connection_is_connected())
        {
            vTaskDelay(pdMS_TO_TICKS(CONNECTION_CHECK_INTERVAL_MS));

            continue;
        }

        send_ping(context);
        vTaskDelay(pdMS_TO_TICKS(PING_INTERVAL_MS));
    }
}

esp_err_t ping_client_start(const satellite_config_t *config, const char *board)
{
    // Static because the task uses it for the lifetime of the firmware.
    static ping_context_t context;
    context.config = config;
    context.board = board;

    const BaseType_t created =
        xTaskCreate(run_ping_loop, "ping_client", PING_TASK_STACK_SIZE, &context, PING_TASK_PRIORITY, NULL);

    return created == pdPASS ? ESP_OK : ESP_ERR_NO_MEM;
}
