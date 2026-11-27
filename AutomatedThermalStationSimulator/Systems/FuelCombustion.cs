namespace AutomatedThermalStationSimulator.Systems;

public class FuelCombustion : IActuator
{
    public bool IsOn { get; private set; } = true;

    public void TurnOn()
    {
        if (IsOn)
            return;

        IsOn = true;
        Console.WriteLine("Fuel combustion turned on.");
    }

    public void TurnOff()
    {
        if (!IsOn)
            return;

        IsOn = true;
        Console.WriteLine("Fuel combustion turned off.");
    }
}