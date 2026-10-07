#include "status_led.h"

#include "esp_timer.h"
#include "freertos/FreeRTOS.h"
#include "freertos/semphr.h"
#include "freertos/task.h"
#include "status_led_policy.h"

#define MICROSECONDS_PER_MILLISECOND 1000

// Short against the 500 ms flash: a request that arrives just after a refresh still gets nearly its full
// flash, and the lamp goes out again close enough to the 500 ms.
#define REFRESH_INTERVAL_MS 50

// The task only reads a few numbers and writes three pins, so it stays well below the default stack.
#define REFRESH_TASK_STACK_SIZE 2048
#define REFRESH_TASK_PRIORITY 3

static gpio_num_t blue_gpio = GPIO_NUM_NC;
static gpio_num_t green_gpio = GPIO_NUM_NC;
static gpio_num_t red_gpio = GPIO_NUM_NC;

static SemaphoreHandle_t lock = NULL;
static bool has_succeeded = false;
static int64_t last_success_ms = 0;
static bool has_requested = false;
static int64_t last_request_ms = 0;

static int64_t now_ms(void)
{
    return esp_timer_get_time() / MICROSECONDS_PER_MILLISECOND;
}

static esp_err_t configure_pins(void)
{
    const gpio_config_t configuration = {
        .pin_bit_mask = (1ULL << blue_gpio) | (1ULL << green_gpio) | (1ULL << red_gpio),
        .mode = GPIO_MODE_OUTPUT,
        .pull_up_en = GPIO_PULLUP_DISABLE,
        .pull_down_en = GPIO_PULLDOWN_DISABLE,
        .intr_type = GPIO_INTR_DISABLE,
    };

    return gpio_config(&configuration);
}

static void show(const status_led_state_t *state)
{
    gpio_set_level(blue_gpio, state->blue);
    gpio_set_level(green_gpio, state->green);
    gpio_set_level(red_gpio, state->red);
}

static void run_refresh_loop(void *argument)
{
    (void)argument;

    while (true)
    {
        status_led_activity_t activity = {.now_ms = now_ms()};

        xSemaphoreTake(lock, portMAX_DELAY);
        activity.has_succeeded = has_succeeded;
        activity.last_success_ms = last_success_ms;
        activity.has_requested = has_requested;
        activity.last_request_ms = last_request_ms;
        xSemaphoreGive(lock);

        const status_led_state_t state = status_led_policy_evaluate(&activity);
        show(&state);

        vTaskDelay(pdMS_TO_TICKS(REFRESH_INTERVAL_MS));
    }
}

esp_err_t status_led_start(gpio_num_t blue, gpio_num_t green, gpio_num_t red)
{
    blue_gpio = blue;
    green_gpio = green;
    red_gpio = red;

    esp_err_t result = configure_pins();
    if (result != ESP_OK)
    {
        return result;
    }

    // Blue right away, and red with it: without a successful communication the satellite is offline by
    // definition, and the first refresh would only repeat that.
    const status_led_state_t initial = {.blue = true, .green = false, .red = true};
    show(&initial);

    lock = xSemaphoreCreateMutex();
    if (lock == NULL)
    {
        return ESP_ERR_NO_MEM;
    }

    const BaseType_t created =
        xTaskCreate(run_refresh_loop, "status_led", REFRESH_TASK_STACK_SIZE, NULL, REFRESH_TASK_PRIORITY, NULL);
    if (created != pdPASS)
    {
        vSemaphoreDelete(lock);
        lock = NULL;

        return ESP_ERR_NO_MEM;
    }

    return ESP_OK;
}

void status_led_note_request(bool succeeded)
{
    // The lamps are optional: could they not be started, the satellite keeps measuring and reporting.
    if (lock == NULL)
    {
        return;
    }

    const int64_t at = now_ms();

    xSemaphoreTake(lock, portMAX_DELAY);
    last_request_ms = at;
    has_requested = true;
    if (succeeded)
    {
        last_success_ms = at;
        has_succeeded = true;
    }
    xSemaphoreGive(lock);
}
