namespace AutomatedThermalStationSimulator;
public class BoilerTemperatureSensor : Sensor
{
    public BoilerTemperatureSensor(string name) : base(name)
    {
    }

    public override void ReadValue()
    {
        Value = new Random().NextDouble() * 600;
        Console.WriteLine($"{Name} measured temperature in boiler: {Value:F2}°C");
    }
}