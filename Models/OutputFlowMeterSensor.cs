namespace Models;

public class OutputFlowMeterSensor : Sensor
{
    public OutputFlowMeterSensor(string name) : base(name)
    {
    }

    public override void ReadValue()
    {
        Value = new Random().NextDouble() * 100000;
        Console.WriteLine($"{Name} measured output flow rate: {Value:F2} W");
    }
}