namespace Models.Entities;

public class SensorReading
{
    public string Topic { get; set; } = "";
    public double Value { get; set; }
    public string Unit { get; set; } = "";
    public DateTimeOffset MeasuredAtUtc { get; set; }
}