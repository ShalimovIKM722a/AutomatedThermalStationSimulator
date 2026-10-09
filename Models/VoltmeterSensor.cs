namespace Models;

public class VoltmeterSensor : Sensor
{
    private readonly AttorchCoulometer _device;

    public VoltmeterSensor(string name, AttorchCoulometer device) : base(name)
    {
        _device = device;
    }

    public override void ReadValue()
    {
        Value = _device.Voltage;
        Console.WriteLine($"{Name} measured voltage: {Value:F2}V");
    }
}