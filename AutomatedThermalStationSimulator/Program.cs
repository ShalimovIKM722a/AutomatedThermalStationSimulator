using AutomatedThermalStationSimulator;
using System.Xml;

public class Program
{

    public static async Task Main(string[] args)
    {
        var thermalSensor = new ThermalSensor("thermal");
        var boilerSensor = new BoilerTemperatureSensor("boiler");
        var currentOutputSensor = new CurrentOutputSensor("current");
        var outputFlowMeterSensor = new OutputFlowMeterSensor("output");
        var steamSensor = new SteamSensor("steam");
        var turbineRotationSensor = new TurbineRotationSensor("turbine");
        var voltageOutputSensor = new VoltageOutputSensor("voltage");
        var waterLevelSensor = new WaterLevelSensor("water");

        var attorch = new AttorchCoulometer(
            "192.168.1.226",
            "yoOpb8-vX_b:xr~F"
        );

        var voltmeterSensor = new VoltmeterSensor(
            "voltmeter",
            attorch
        );
        var currentSensor = new CurrentSensor(
            "current",
            attorch
        );

        var socSensor = new SocSensor(
            "soc",
            attorch
        );

        await attorch.ReadStatusAsync();

        ThermalStation station = new ThermalStation();

        station.Sensors.Add(thermalSensor);
        station.Sensors.Add(boilerSensor);
        station.Sensors.Add(currentOutputSensor);
        station.Sensors.Add(outputFlowMeterSensor);
        station.Sensors.Add(steamSensor);
        station.Sensors.Add(turbineRotationSensor);
        station.Sensors.Add(voltageOutputSensor);
        station.Sensors.Add(waterLevelSensor);
        station.Sensors.Add(voltmeterSensor);
        station.Sensors.Add(currentSensor);
        station.Sensors.Add(socSensor);


        station.Monitor();

        if (steamSensor.Value > 10)
        {
            station.FuelCombustionSystem.TurnOn();
        }
        else
        {
            station.FuelCombustionSystem.TurnOff();
        }


    }
}