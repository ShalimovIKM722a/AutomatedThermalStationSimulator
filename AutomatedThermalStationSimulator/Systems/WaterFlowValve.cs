namespace AutomatedThermalStationSimulator.Systems;

public class WaterFlowValve
{
    public bool Open { get; set; }

    public void OpenValve()
    {
        Open = true;
        Console.WriteLine("Valve opened.");
    }

    public void CloseValve()
    {
        Open = false;
        Console.WriteLine("Valve closed.");
    }
}
