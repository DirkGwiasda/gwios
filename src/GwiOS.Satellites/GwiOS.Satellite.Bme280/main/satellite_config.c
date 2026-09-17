#include "satellite_config.h"

#include <stdbool.h>
#include <stddef.h>

#include "esp_log.h"
#include "nvs.h"
#include "nvs_flash.h"

// Partition and namespace names must match partitions.csv and the configuration CSV.
#define CONFIG_PARTITION "config"
#define CONFIG_NAMESPACE "satellite"

static const char *TAG = "satellite_config";

// One configuration entry: where it lives in NVS, where it goes in memory and which rule it must follow.
typedef struct
{
    const char *key;
    char *buffer;
    size_t buffer_size;
    bool (*is_valid)(const char *value);
} config_entry_t;

static esp_err_t open_config_namespace(nvs_handle_t *out_handle)
{
    esp_err_t result = nvs_flash_init_partition(CONFIG_PARTITION);
    if (result == ESP_ERR_NOT_FOUND)
    {
        ESP_LOGE(TAG, "Partition '%s' not found. Is the partition table from partitions.csv flashed?", CONFIG_PARTITION);

        return result;
    }

    if (result != ESP_OK)
    {
        ESP_LOGE(TAG, "Partition '%s' cannot be read: %s.", CONFIG_PARTITION, esp_err_to_name(result));

        return result;
    }

    result = nvs_open_from_partition(CONFIG_PARTITION, CONFIG_NAMESPACE, NVS_READONLY, out_handle);
    if (result == ESP_ERR_NVS_NOT_FOUND)
    {
        ESP_LOGE(TAG, "No configuration flashed (namespace '%s' is empty).", CONFIG_NAMESPACE);

        return ESP_ERR_NOT_FOUND;
    }

    return result;
}

static esp_err_t load_entry(nvs_handle_t handle, const config_entry_t *entry)
{
    size_t length = entry->buffer_size;
    esp_err_t result = nvs_get_str(handle, entry->key, entry->buffer, &length);
    if (result == ESP_ERR_NVS_NOT_FOUND)
    {
        ESP_LOGE(TAG, "Entry '%s' is missing.", entry->key);

        return ESP_ERR_NOT_FOUND;
    }

    if (result == ESP_ERR_NVS_INVALID_LENGTH)
    {
        ESP_LOGE(TAG, "Entry '%s' is too long (at most %u characters).", entry->key, (unsigned)(entry->buffer_size - 1));

        return ESP_ERR_INVALID_ARG;
    }

    if (result != ESP_OK)
    {
        ESP_LOGE(TAG, "Entry '%s' cannot be read: %s.", entry->key, esp_err_to_name(result));

        return result;
    }

    if (!entry->is_valid(entry->buffer))
    {
        ESP_LOGE(TAG, "Entry '%s' is invalid, see the rules in satellite_config_rules.h.", entry->key);

        return ESP_ERR_INVALID_ARG;
    }

    return ESP_OK;
}

esp_err_t satellite_config_load(satellite_config_t *out_config)
{
    nvs_handle_t handle = 0;
    esp_err_t result = open_config_namespace(&handle);
    if (result != ESP_OK)
    {
        return result;
    }

    const config_entry_t entries[] = {
        {"name", out_config->name, sizeof(out_config->name), satellite_config_is_valid_name},
        {"token", out_config->token, sizeof(out_config->token), satellite_config_is_valid_token},
        {"wifi_ssid", out_config->wifi_ssid, sizeof(out_config->wifi_ssid), satellite_config_is_valid_wifi_ssid},
        {"wifi_password", out_config->wifi_password, sizeof(out_config->wifi_password), satellite_config_is_valid_wifi_password},
        {"gwios_url", out_config->gwios_url, sizeof(out_config->gwios_url), satellite_config_is_valid_gwios_url},
    };

    // Every entry is checked even after the first failure, so a single flash cycle reveals all problems.
    esp_err_t overall = ESP_OK;
    for (size_t index = 0; index < sizeof(entries) / sizeof(entries[0]); index++)
    {
        result = load_entry(handle, &entries[index]);
        if (result != ESP_OK && overall == ESP_OK)
        {
            overall = result;
        }
    }

    nvs_close(handle);

    return overall;
}
