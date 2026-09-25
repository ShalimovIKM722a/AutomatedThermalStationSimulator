namespace AutomatedThermalStationSimulator;

public class WaterLevelSensor : Sensor
{
    public WaterLevelSensor(string name) : base(name)
    {
    }

    public override void ReadValue()
    {
        Value = new Random().NextDouble() * 100;
        Console.WriteLine($"{Name} measured water level: {Value:F2} cm");
    }
}