namespace Models;

public class SteamSensor : Sensor
{
    public SteamSensor(string name) : base(name)
    {
    }
    public override void ReadValue()
    {
        Value = new Random().NextDouble() * 10;
        Console.WriteLine($"{Name} measured steam pressure: {Value:F2} bar");
    }
}
