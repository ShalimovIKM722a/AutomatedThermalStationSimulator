namespace AutomatedThermalStationSimulator;

class TurbineRotationSensor : Sensor
{
    public TurbineRotationSensor(string name) : base(name)
    {
    }
    public override void ReadValue()
    {
        Value = new Random().NextDouble() * 1000; 
        Console.WriteLine($"{Name} measured turbine rotation speed: {Value:F2} RPM");
    }
}