#include "wifi_connection.h"

#include <stdatomic.h>
#include <string.h>

#include "esp_check.h"
#include "esp_event.h"
#include "esp_log.h"
#include "esp_mac.h"
#include "esp_netif.h"
#include "esp_timer.h"
#include "esp_wifi.h"
#include "nvs_flash.h"
#include "wifi_reconnect_policy.h"

static const char *TAG = "wifi_connection";

static esp_netif_t *station = NULL;
static esp_timer_handle_t reconnect_timer = NULL;

// Written by the event handlers, read by other tasks through the public functions.
static atomic_bool connected = false;

// Only touched by the event handlers, which all run in the default event loop task.
static uint32_t failed_attempts = 0;

static esp_err_t init_default_nvs(void)
{
    // The Wi-Fi driver keeps its calibration and connection data in the default "nvs" partition.
    esp_err_t result = nvs_flash_init();
    if (result == ESP_ERR_NVS_NO_FREE_PAGES || result == ESP_ERR_NVS_NEW_VERSION_FOUND)
    {
        // Only the driver's own data lives there (the configuration has its own partition), so starting over
        // loses nothing that cannot be rebuilt.
        ESP_LOGW(TAG, "Default NVS partition is unusable (%s), erasing it.", esp_err_to_name(result));
        ESP_RETURN_ON_ERROR(nvs_flash_erase(), TAG, "Erasing the default NVS partition failed");
        result = nvs_flash_init();
    }

    return result;
}

static void connect_now(void *argument)
{
    (void)argument;

    esp_err_t result = esp_wifi_connect();
    if (result != ESP_OK)
    {
        ESP_LOGE(TAG, "Starting a connection attempt failed: %s.", esp_err_to_name(result));
    }
}

static void schedule_reconnect(void)
{
    failed_attempts++;
    const uint32_t delay_ms = wifi_reconnect_delay_ms(failed_attempts);

    ESP_LOGW(TAG, "Retrying in %u s (attempt %u).", (unsigned)(delay_ms / 1000), (unsigned)failed_attempts);

    esp_err_t result = esp_timer_start_once(reconnect_timer, (uint64_t)delay_ms * 1000);
    if (result != ESP_OK)
    {
        ESP_LOGE(TAG, "Scheduling the reconnect failed: %s.", esp_err_to_name(result));
    }
}

static void on_wifi_event(void *argument, esp_event_base_t base, int32_t id, void *data)
{
    (void)argument;
    (void)base;

    if (id == WIFI_EVENT_STA_START)
    {
        connect_now(NULL);
    }
    else if (id == WIFI_EVENT_STA_CONNECTED)
    {
        const wifi_event_sta_connected_t *event = data;

        // In a mesh all access points share the SSID; the BSSID tells which one the satellite chose.
        ESP_LOGI(TAG, "Associated with access point " MACSTR " on channel %u.", MAC2STR(event->bssid), event->channel);
    }
    else if (id == WIFI_EVENT_STA_DISCONNECTED)
    {
        const wifi_event_sta_disconnected_t *event = data;

        atomic_store(&connected, false);
        // The reason code tells a wrong password (15, 204) from an access point that is out of reach (201).
        ESP_LOGW(TAG, "Not connected to '%.*s' (reason %u).", event->ssid_len, (const char *)event->ssid, event->reason);
        schedule_reconnect();
    }
}

static void on_ip_event(void *argument, esp_event_base_t base, int32_t id, void *data)
{
    (void)argument;
    (void)base;

    if (id == IP_EVENT_STA_GOT_IP)
    {
        const ip_event_got_ip_t *event = data;

        failed_attempts = 0;
        atomic_store(&connected, true);

        int8_t rssi_dbm = 0;
        wifi_connection_get_rssi(&rssi_dbm);
        ESP_LOGI(TAG, "Connected, IP " IPSTR ", signal %d dBm.", IP2STR(&event->ip_info.ip), rssi_dbm);
    }
    else if (id == IP_EVENT_STA_LOST_IP)
    {
        atomic_store(&connected, false);
        ESP_LOGW(TAG, "IP address lost.");
    }
}

static esp_err_t create_station(const char *hostname)
{
    ESP_RETURN_ON_ERROR(esp_netif_init(), TAG, "Initialising the network interface layer failed");
    ESP_RETURN_ON_ERROR(esp_event_loop_create_default(), TAG, "Creating the default event loop failed");

    station = esp_netif_create_default_wifi_sta();
    ESP_RETURN_ON_FALSE(station != NULL, ESP_FAIL, TAG, "Creating the station interface failed");

    return esp_netif_set_hostname(station, hostname);
}

static esp_err_t create_reconnect_timer(void)
{
    const esp_timer_create_args_t timer_args = {
        .callback = connect_now,
        .name = "wifi_reconnect",
    };

    return esp_timer_create(&timer_args, &reconnect_timer);
}

static esp_err_t register_event_handlers(void)
{
    ESP_RETURN_ON_ERROR(esp_event_handler_instance_register(WIFI_EVENT, ESP_EVENT_ANY_ID, on_wifi_event, NULL, NULL),
                        TAG,
                        "Registering the Wi-Fi event handler failed");

    return esp_event_handler_instance_register(IP_EVENT, ESP_EVENT_ANY_ID, on_ip_event, NULL, NULL);
}

static esp_err_t configure_driver(const char *ssid, const char *password)
{
    const wifi_init_config_t init_config = WIFI_INIT_CONFIG_DEFAULT();
    ESP_RETURN_ON_ERROR(esp_wifi_init(&init_config), TAG, "Initialising the Wi-Fi driver failed");

    wifi_config_t config = {0};
    // The configuration rules guarantee that both values fit; an SSID of exactly 32 bytes needs no terminator.
    strncpy((char *)config.sta.ssid, ssid, sizeof(config.sta.ssid));
    strncpy((char *)config.sta.password, password, sizeof(config.sta.password) - 1);
    // Refuse to fall back to a weaker network of the same name than the one configured.
    config.sta.threshold.authmode = password[0] == '\0' ? WIFI_AUTH_OPEN : WIFI_AUTH_WPA2_PSK;
    // A fast scan takes the first access point with the SSID, which in a mesh may be the distant router instead of the
    // nearby repeater. Scanning all channels and picking the strongest costs about a second per connection.
    config.sta.scan_method = WIFI_ALL_CHANNEL_SCAN;
    config.sta.sort_method = WIFI_CONNECT_AP_BY_SIGNAL;

    ESP_RETURN_ON_ERROR(esp_wifi_set_mode(WIFI_MODE_STA), TAG, "Setting station mode failed");
    ESP_RETURN_ON_ERROR(esp_wifi_set_config(WIFI_IF_STA, &config), TAG, "Setting the Wi-Fi configuration failed");

    // Modem sleep delays incoming requests until the next beacon and occasionally drops them. The satellite
    // serves GwiOS over HTTP and runs on USB power, so responsiveness wins over power draw.
    return esp_wifi_set_ps(WIFI_PS_NONE);
}

esp_err_t wifi_connection_start(const char *hostname, const char *ssid, const char *password)
{
    ESP_RETURN_ON_ERROR(init_default_nvs(), TAG, "Initialising the default NVS partition failed");
    ESP_RETURN_ON_ERROR(create_station(hostname), TAG, "Creating the station failed");
    ESP_RETURN_ON_ERROR(create_reconnect_timer(), TAG, "Creating the reconnect timer failed");
    ESP_RETURN_ON_ERROR(register_event_handlers(), TAG, "Registering the event handlers failed");
    ESP_RETURN_ON_ERROR(configure_driver(ssid, password), TAG, "Configuring the Wi-Fi driver failed");

    ESP_LOGI(TAG, "Connecting to '%s' as '%s'.", ssid, hostname);

    return esp_wifi_start();
}

bool wifi_connection_is_connected(void)
{
    return atomic_load(&connected);
}

esp_err_t wifi_connection_get_ip_address(esp_ip4_addr_t *out_address)
{
    if (!wifi_connection_is_connected())
    {
        return ESP_ERR_INVALID_STATE;
    }

    esp_netif_ip_info_t ip_info;
    ESP_RETURN_ON_ERROR(esp_netif_get_ip_info(station, &ip_info), TAG, "Reading the IP address failed");
    *out_address = ip_info.ip;

    return ESP_OK;
}

esp_err_t wifi_connection_get_rssi(int8_t *out_rssi_dbm)
{
    int rssi = 0;
    esp_err_t result = esp_wifi_sta_get_rssi(&rssi);
    if (result != ESP_OK)
    {
        return ESP_ERR_INVALID_STATE;
    }

    *out_rssi_dbm = (int8_t)rssi;

    return ESP_OK;
}
