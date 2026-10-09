using Models;
namespace AutomatedThermalStationSimulator.Mqtt;


public class SensorConfiguration
{
    public Sensor Sensor { get; set; }
    public string Topic { get; set; }
    public string Unit { get; set; }

    public SensorConfiguration(Sensor sensor, string topic, string unit)
    {
        Sensor = sensor;
        Topic = topic;
        Unit = unit;
    }
}