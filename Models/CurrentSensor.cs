using Models;

public class CurrentSensor : Sensor
{
    private readonly AttorchCoulometer _device;
    private readonly Random _random = new();

    public CurrentSensor(
        string name,
        AttorchCoulometer device) : base(name)
    {
        _device = device;
    }

    public override void ReadValue()
    {
        if (_device.HasData)
        {
            Value = _device.Current;
        }
        else
        {
            Value = Math.Round(
                _random.NextDouble() * 10, 2);
        }
    }
}