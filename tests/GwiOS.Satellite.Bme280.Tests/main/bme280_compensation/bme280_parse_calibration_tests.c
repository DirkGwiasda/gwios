#include "bme280_compensation.h"
#include "unity.h"

void setUp(void)
{
}

void tearDown(void)
{
}

// Register contents 0x88 to 0xA1 for the datasheet example calibration (little-endian words), with
// H1 = 75 in the last byte.
static const uint8_t FIRST_BLOCK[BME280_CALIBRATION_FIRST_BLOCK_LENGTH] = {
    0x70, 0x6B, 0x43, 0x67, 0x18, 0xFC, 0x7D, 0x8E, 0x43, 0xD6, 0xD0, 0x0B, 0x27,
    0x0B, 0x8C, 0x00, 0xF9, 0xFF, 0x8C, 0x3C, 0xF8, 0xC6, 0x70, 0x17, 0x00, 0x4B,
};

static void test_decodes_temperature_words_including_negative_values(void)
{
    const uint8_t second_block[BME280_CALIBRATION_SECOND_BLOCK_LENGTH] = {0};
    bme280_calibration_t calibration;

    bme280_parse_calibration(FIRST_BLOCK, second_block, &calibration);

    TEST_ASSERT_EQUAL_UINT16(27504, calibration.t1);
    TEST_ASSERT_EQUAL_INT16(26435, calibration.t2);
    TEST_ASSERT_EQUAL_INT16(-1000, calibration.t3);
}

static void test_decodes_all_pressure_words(void)
{
    const uint8_t second_block[BME280_CALIBRATION_SECOND_BLOCK_LENGTH] = {0};
    bme280_calibration_t calibration;

    bme280_parse_calibration(FIRST_BLOCK, second_block, &calibration);

    TEST_ASSERT_EQUAL_UINT16(36477, calibration.p1);
    TEST_ASSERT_EQUAL_INT16(-10685, calibration.p2);
    TEST_ASSERT_EQUAL_INT16(3024, calibration.p3);
    TEST_ASSERT_EQUAL_INT16(2855, calibration.p4);
    TEST_ASSERT_EQUAL_INT16(140, calibration.p5);
    TEST_ASSERT_EQUAL_INT16(-7, calibration.p6);
    TEST_ASSERT_EQUAL_INT16(15500, calibration.p7);
    TEST_ASSERT_EQUAL_INT16(-14600, calibration.p8);
    TEST_ASSERT_EQUAL_INT16(6000, calibration.p9);
}

static void test_takes_h1_from_the_last_byte_of_the_first_block(void)
{
    const uint8_t second_block[BME280_CALIBRATION_SECOND_BLOCK_LENGTH] = {0};
    bme280_calibration_t calibration;

    bme280_parse_calibration(FIRST_BLOCK, second_block, &calibration);

    TEST_ASSERT_EQUAL_UINT8(75, calibration.h1);
}

static void test_decodes_humidity_values_from_the_second_block(void)
{
    // H2 = 362, H3 = 0, H4 = 313 (0x139), H5 = 50 (0x032), H6 = 30. Register 0xE5 carries the low nibble
    // of H4 in its low half and the low nibble of H5 in its high half.
    const uint8_t second_block[BME280_CALIBRATION_SECOND_BLOCK_LENGTH] = {0x6A, 0x01, 0x00, 0x13, 0x29, 0x03, 0x1E};
    bme280_calibration_t calibration;

    bme280_parse_calibration(FIRST_BLOCK, second_block, &calibration);

    TEST_ASSERT_EQUAL_INT16(362, calibration.h2);
    TEST_ASSERT_EQUAL_UINT8(0, calibration.h3);
    TEST_ASSERT_EQUAL_INT16(313, calibration.h4);
    TEST_ASSERT_EQUAL_INT16(50, calibration.h5);
    TEST_ASSERT_EQUAL_INT8(30, calibration.h6);
}

static void test_decodes_negative_twelve_bit_h4_and_h5(void)
{
    // H4 = -5 (0xFFB) and H5 = -2 (0xFFE): the sign lives in the full byte, the nibbles in register 0xE5.
    const uint8_t second_block[BME280_CALIBRATION_SECOND_BLOCK_LENGTH] = {0x00, 0x00, 0x00, 0xFF, 0xEB, 0xFF, 0xF6};
    bme280_calibration_t calibration;

    bme280_parse_calibration(FIRST_BLOCK, second_block, &calibration);

    TEST_ASSERT_EQUAL_INT16(-5, calibration.h4);
    TEST_ASSERT_EQUAL_INT16(-2, calibration.h5);
    TEST_ASSERT_EQUAL_INT8(-10, calibration.h6);
}

int main(void)
{
    UNITY_BEGIN();
    RUN_TEST(test_decodes_temperature_words_including_negative_values);
    RUN_TEST(test_decodes_all_pressure_words);
    RUN_TEST(test_takes_h1_from_the_last_byte_of_the_first_block);
    RUN_TEST(test_decodes_humidity_values_from_the_second_block);
    RUN_TEST(test_decodes_negative_twelve_bit_h4_and_h5);
    return UNITY_END();
}
