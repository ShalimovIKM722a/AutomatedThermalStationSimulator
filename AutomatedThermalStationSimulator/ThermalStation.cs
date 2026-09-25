using AutomatedThermalStationSimulator.Systems;

namespace AutomatedThermalStationSimulator;
public class ThermalStation
{
    public List<Sensor> Sensors { get; set; }
    public FuelCombustion FuelCombustionSystem { get; set; }


    public ThermalStation()
    {
        Sensors = new List<Sensor>();
        FuelCombustionSystem = new FuelCombustion();
    }

    public void Monitor()
    {
        foreach (Sensor sensor in Sensors)
        {
            sensor.ReadValue();
        }
    }
}