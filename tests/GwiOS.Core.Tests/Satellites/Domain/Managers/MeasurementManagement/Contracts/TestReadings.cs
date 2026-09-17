using GwiOS.Core.Satellites.Domain.Managers.MeasurementManagement.Contracts.Models;

namespace GwiOS.Core.Tests.Satellites.Domain.Managers.MeasurementManagement.Contracts;

/// <summary>
/// Creates readings as the BME280 satellite delivers them.
/// </summary>
public sealed class TestReadings
{
    /// <summary>
    /// Returns a reading with temperature, humidity and pressure measured the given seconds ago.
    /// </summary>
    public MeasurementReading Create(double temperature = 24.2, int ageSeconds = 3)
        => new()
        {
            Measurements =
            [
                new Measurement { Sensor = "bme280", Quantity = "temperature", Value = temperature, Unit = "°C" },
                new Measurement { Sensor = "bme280", Quantity = "humidity", Value = 50.64, Unit = "%" },
                new Measurement { Sensor = "bme280", Quantity = "pressure", Value = 996.37, Unit = "hPa" }
            ],
            AgeSeconds = ageSeconds
        };
}
