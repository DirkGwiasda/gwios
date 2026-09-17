#include "measurement_store.h"

#include "esp_timer.h"
#include "freertos/FreeRTOS.h"
#include "freertos/semphr.h"

#define MICROSECONDS_PER_SECOND 1000000

static SemaphoreHandle_t lock = NULL;
static bme280_measurement_t latest;
static int64_t taken_at_us = 0;
static bool has_measurement = false;

esp_err_t measurement_store_init(void)
{
    lock = xSemaphoreCreateMutex();

    return lock != NULL ? ESP_OK : ESP_ERR_NO_MEM;
}

void measurement_store_put(const bme280_measurement_t *measurement)
{
    xSemaphoreTake(lock, portMAX_DELAY);
    latest = *measurement;
    taken_at_us = esp_timer_get_time();
    has_measurement = true;
    xSemaphoreGive(lock);
}

bool measurement_store_get(bme280_measurement_t *out_measurement, uint32_t *out_age_seconds)
{
    xSemaphoreTake(lock, portMAX_DELAY);
    const bool available = has_measurement;
    if (available)
    {
        *out_measurement = latest;
        *out_age_seconds = (uint32_t)((esp_timer_get_time() - taken_at_us) / MICROSECONDS_PER_SECOND);
    }
    xSemaphoreGive(lock);

    return available;
}
