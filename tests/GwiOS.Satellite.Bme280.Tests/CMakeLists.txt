# Host tests for the pure logic of the BME280 satellite firmware. They build with the host compiler and
# run without a board; hardware-bound code is checked on the board instead (see the satellite README).
cmake_minimum_required(VERSION 3.16)

project(gwios_satellite_bme280_tests C)

set(CMAKE_C_STANDARD 11)
set(CMAKE_C_STANDARD_REQUIRED ON)

set(FIRMWARE_MAIN_DIR "${CMAKE_CURRENT_LIST_DIR}/../../src/GwiOS.Satellites/GwiOS.Satellite.Bme280/main")

# Unity ships with ESP-IDF, so the tests need no copy of their own. Configure from a plain terminal, not
# an ESP-IDF one: its PATH puts the linker of the ESP32 ULP coprocessor ahead of the host linker.
set(UNITY_DIR "$ENV{HOME}/.espressif/v6.1/esp-idf/components/unity/unity/src" CACHE PATH "Directory containing unity.c")
if(NOT EXISTS "${UNITY_DIR}/unity.c")
    message(FATAL_ERROR "Unity not found in '${UNITY_DIR}'. Pass -DUNITY_DIR=<esp-idf>/components/unity/unity/src.")
endif()

add_library(unity STATIC "${UNITY_DIR}/unity.c")
target_include_directories(unity PUBLIC "${UNITY_DIR}")
# Double comparisons are off in Unity by default; the humidity tests compare against a floating-point oracle.
target_compile_definitions(unity PUBLIC UNITY_INCLUDE_DOUBLE)

enable_testing()

# Adds one test executable per tested function, named after its source file.
function(add_firmware_test test_source firmware_source)
    get_filename_component(test_name "${test_source}" NAME_WE)
    add_executable(${test_name} "${test_source}" "${FIRMWARE_MAIN_DIR}/${firmware_source}")
    target_include_directories(${test_name} PRIVATE "${FIRMWARE_MAIN_DIR}" "${CMAKE_CURRENT_LIST_DIR}")
    target_compile_options(${test_name} PRIVATE -Wall -Wextra)
    target_link_libraries(${test_name} PRIVATE unity m)
    add_test(NAME ${test_name} COMMAND ${test_name})
endfunction()

add_firmware_test(main/bme280_compensation/bme280_parse_calibration_tests.c bme280_compensation.c)
add_firmware_test(main/bme280_compensation/bme280_parse_raw_measurement_tests.c bme280_compensation.c)
add_firmware_test(main/bme280_compensation/bme280_compensate_temperature_tests.c bme280_compensation.c)
add_firmware_test(main/bme280_compensation/bme280_compensate_pressure_tests.c bme280_compensation.c)
add_firmware_test(main/bme280_compensation/bme280_compensate_humidity_tests.c bme280_compensation.c)
add_firmware_test(main/satellite_config_rules/satellite_config_is_valid_name_tests.c satellite_config_rules.c)
add_firmware_test(main/satellite_config_rules/satellite_config_is_valid_token_tests.c satellite_config_rules.c)
add_firmware_test(main/satellite_config_rules/satellite_config_is_valid_wifi_ssid_tests.c satellite_config_rules.c)
add_firmware_test(main/satellite_config_rules/satellite_config_is_valid_wifi_password_tests.c satellite_config_rules.c)
add_firmware_test(main/satellite_config_rules/satellite_config_is_valid_gwios_url_tests.c satellite_config_rules.c)
add_firmware_test(main/wifi_reconnect_policy/wifi_reconnect_delay_ms_tests.c wifi_reconnect_policy.c)
add_firmware_test(main/bearer_token/bearer_token_matches_tests.c bearer_token.c)
add_firmware_test(main/measurements_json/measurements_json_write_tests.c measurements_json.c)
add_firmware_test(main/ping_url/ping_url_write_tests.c ping_url.c)
add_firmware_test(main/ping_payload/ping_payload_write_tests.c ping_payload.c)
add_firmware_test(main/status_led_policy/status_led_policy_evaluate_tests.c status_led_policy.c)
