namespace AutomatedThermalStationSimulator;

public class CurrentOutputSensor : Sensor
{
    public CurrentOutputSensor(string name) : base(name)
    {
    }
    public override void ReadValue()
    {
        Value = new Random().NextDouble() * 10;
        Console.WriteLine($"{Name} measured current output: {Value:F2} A");
    }
}