using Models;

public class VoltmeterSensor : Sensor
{
    private readonly AttorchCoulometer _device;
    private readonly Random _random = new();

    public VoltmeterSensor(
        string name,
        AttorchCoulometer device) : base(name)
    {
        _device = device;
    }

    public override void ReadValue()
    {
        if (_device.HasData)
        {
            Value = _device.Voltage;
        }
        else
        {
            Value = _random.Next(20, 220);
        }
    }
}
