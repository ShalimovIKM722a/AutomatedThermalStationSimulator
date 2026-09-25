namespace AutomatedThermalStationSimulator.Systems;

public class FuelCombustion
{
    public bool IsOn { get; set; }

    public void TurnOn()
    {
        IsOn = true;
        Console.WriteLine("Fuel combustion turned on.");
    }

    public void TurnOff()
    {
        IsOn = false;
        Console.WriteLine("Fuel combustion turned off.");
    }
}
