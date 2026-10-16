using Models;

public class SocSensor : Sensor
{
    private readonly AttorchCoulometer _device;
    private readonly Random _random = new();

    public SocSensor(
        string name,
        AttorchCoulometer device) : base(name)
    {
        _device = device;
    }

    public override void ReadValue()
    {
        if (_device.HasData)
        {
            Value = _device.Capacity;
        }
        else
        {
            Value = _random.Next(0, 100);
        }
    }
}
