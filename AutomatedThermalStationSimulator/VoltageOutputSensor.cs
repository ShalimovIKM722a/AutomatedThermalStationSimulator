namespace AutomatedThermalStationSimulator;

public class VoltageOutputSensor : Sensor
{
    public VoltageOutputSensor(string name) : base(name)
    {
    }
    public override void ReadValue()
    {
        Value = new Random().NextDouble() * 240;
        Console.WriteLine($"{Name} measured voltage output: {Value:F2} V");
    }
}