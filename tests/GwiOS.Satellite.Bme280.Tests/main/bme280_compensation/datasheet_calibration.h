#pragma once

#include "bme280_compensation.h"

// Temperature and pressure calibration from the worked example in the Bosch datasheet (BMP280, section
// 3.12; the BME280 uses the same formulas). Raw temperature 519888 gives 25.08 degrees Celsius and
// t_fine 128422, raw pressure 415148 then gives 100653 Pascal.
#define DATASHEET_RAW_TEMPERATURE 519888
#define DATASHEET_RAW_PRESSURE 415148
#define DATASHEET_T_FINE 128422

// The datasheet has no humidity example. These values are typical for a real BME280 and serve as input
// for comparing the integer formula against the floating-point reference formula.
static inline bme280_calibration_t datasheet_calibration(void)
{
    const bme280_calibration_t calibration = {
        .t1 = 27504,
        .t2 = 26435,
        .t3 = -1000,
        .p1 = 36477,
        .p2 = -10685,
        .p3 = 3024,
        .p4 = 2855,
        .p5 = 140,
        .p6 = -7,
        .p7 = 15500,
        .p8 = -14600,
        .p9 = 6000,
        .h1 = 75,
        .h2 = 362,
        .h3 = 0,
        .h4 = 313,
        .h5 = 50,
        .h6 = 30,
    };

    return calibration;
}
