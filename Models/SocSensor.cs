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
        if (_device.Capacity < 0 || _device.Capacity > 100)
        {
            Value = 100;
        }
        else
        {
            Value = _device.Capacity;
        }
        Console.WriteLine($"{Name} measured state of charge: {Value:F2} %");
    }
}