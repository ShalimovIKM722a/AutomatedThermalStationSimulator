namespace Models;

public class CurrentSensor : Sensor
{
    private readonly AttorchCoulometer _device;

    public CurrentSensor(string name, AttorchCoulometer device) : base(name)
    {
        _device = device;
    }

    public override void ReadValue()
    {
        Value = _device.Current;
        Console.WriteLine($"{Name} measured current: {Value:F2} A");
    }
}