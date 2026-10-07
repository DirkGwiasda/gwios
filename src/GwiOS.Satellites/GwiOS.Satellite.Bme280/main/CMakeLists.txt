idf_component_register(SRCS "wifi_connection.c" "wifi_reconnect_policy.c" "satellite_config.c" "satellite_config_rules.c" "main.c" "bme280.c" "bme280_compensation.c"
                            "bearer_token.c" "measurements_json.c" "measurement_store.c" "measurement_server.c"
                            "ping_url.c" "ping_payload.c" "ping_client.c"
                            "status_led_policy.c" "status_led.c"
                       INCLUDE_DIRS "."
                       REQUIRES esp_app_format esp_driver_gpio esp_driver_i2c esp_http_client esp_http_server mbedtls esp_netif esp_timer esp_wifi nvs_flash)
