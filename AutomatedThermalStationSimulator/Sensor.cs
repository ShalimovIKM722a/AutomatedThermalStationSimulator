namespace AutomatedThermalStationSimulator;
public abstract class Sensor
{
    public string Name { get; set; }

    public double Value { get; protected set; }

    public Sensor(string name)
    {
        Name = name;
    }

    public abstract void ReadValue();
}