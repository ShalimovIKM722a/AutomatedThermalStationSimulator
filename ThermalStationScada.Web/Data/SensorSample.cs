namespace ThermalStationScada.Web.Data;

public class SensorSample
{
    public int Id { get; set; }
    public string Topic { get; set; } = "";
    public double Value { get; set; }
    public string Unit { get; set; } = "";
    public DateTimeOffset MeasuredAtUtc { get; set; }
}
