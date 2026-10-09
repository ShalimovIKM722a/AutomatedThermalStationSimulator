namespace Models;

public class SocSensor : Sensor
{
    private readonly AttorchCoulometer _device;

    public SocSensor(string name, AttorchCoulometer device) : base(name)
    {
        _device = device;
    }

    public override void ReadValue()
    {
        Value = _device.Capacity;
        Console.WriteLine($"{Name} measured state of charge: {Value:F2} %");
    }
}