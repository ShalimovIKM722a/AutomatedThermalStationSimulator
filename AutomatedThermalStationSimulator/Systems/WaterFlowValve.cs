namespace AutomatedThermalStationSimulator.Systems;

public class WaterFlowValve : IActuator
{
    public bool IsOn { get; private set; }

    public void TurnOn()
    {
        if (IsOn)
            return;

        IsOn = true;
        Console.WriteLine("Water flow valve opened.");
    }

    public void TurnOff()
    {
        if (!IsOn)
            return;

        IsOn = false;
        Console.WriteLine("Water flow valve closed.");
    }
}