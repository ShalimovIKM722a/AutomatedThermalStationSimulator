namespace Models.Systems;

public class WaterFlowValve : IActuator
{
    public bool IsOn { get; private set; }

    public void TurnOn()
    {
        IsOn = true;
        Console.WriteLine("Valve turned on.");
    }

    public void TurnOff()
    {
        IsOn = false;
        Console.WriteLine("Valve turned off.");
    }
}
