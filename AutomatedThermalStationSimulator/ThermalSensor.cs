namespace AutomatedThermalStationSimulator;

public class ThermalSensor : Sensor
{
    public ThermalSensor(string name) : base(name)
    {
    }

    public override void ReadValue()
    {
        Value = new Random().NextDouble() * 100;
        Console.WriteLine($"{Name} measured temperature: {Value:F2}°C");
    }
}