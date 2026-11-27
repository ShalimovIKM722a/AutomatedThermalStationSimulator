using AutomatedThermalStationSimulator;
using AutomatedThermalStationSimulator.Mqtt;
using AutomatedThermalStationSimulator.Systems;
using Models;
using MQTTnet;
using System.Text.Json;

public class Program
{
    public static async Task Main()
    {
        SensorConfiguration[] sensors = CreateSensors();
        ActuatorConfiguration[] actuators = CreateActuators(sensors);

        var factory = new MqttClientFactory();
        using var mqttClient = factory.CreateMqttClient();

        var options = new MqttClientOptionsBuilder()
            .WithTcpServer("127.0.0.1", 1883)
            .Build();

        await mqttClient.ConnectAsync(options);
        Console.WriteLine("Connected to MQTT broker.");

        await RunSimulationAsync(mqttClient, sensors, actuators);

        await mqttClient.DisconnectAsync();
    }

    private static SensorConfiguration[] CreateSensors()
    {
        var attorch = new AttorchCoulometer(
          "192.168.1.226",
          "yoOpb8-vX_b:xr~F"
       );
        _ = attorch.StartAsync(
        connectionTimeoutSeconds: 5,
        retryDelayMilliseconds: 3000,
        pollingIntervalMilliseconds: 1000);
        return
       [
           new (
                new BoilerTemperatureSensor("Boiler temperature"),
                "thermalStation/1/sensors/boiler/temperature",
                "°C"),
            new (
                new ThermalSensor("Outside temperature"),
                "thermalStation/1/sensors/outside/thermal",
                "°C"),
            new (
                new WaterLevelSensor("Water level"),
                "thermalStation/1/sensors/boiler/waterLevel",
                "%"),
            new (
                new SteamSensor("Steam pressure"),
                "thermalStation/1/sensors/outside/steam",
                "bar"),
            new (
                new TurbineRotationSensor("Turbine rotation"),
                "thermalStation/1/sensors/turbine/rotation",
                "rpm"),
            new (
                new CurrentSensor("Input current", attorch),
                "thermalStation/1/sensors/inside/currentInput",
                "A"),
            new (
                new CurrentOutputSensor("Output current"),
                "thermalStation/1/sensors/inside/current",
                "A"),
            new (
                new VoltmeterSensor("Output voltage", attorch),
                "thermalStation/1/sensors/output/voltagOutput",
                "V"),
            new (
                new VoltageOutputSensor("Output voltage"),
                "thermalStation/1/sensors/outside/voltage",
                "V"),
            new (
                new SocSensor("Battery charge", attorch),
                "thermalStation/1/sensors/battery/soc",
                "%"),
            new (
                new OutputFlowMeterSensor("Water flow"),
                "thermalStation/1/sensors/boiler/outputFlow",
                "L/min")
       ];
    }

    private static ActuatorConfiguration[] CreateActuators(
    SensorConfiguration[] sensors)
    {
        Sensor boilerTemperature = sensors[0].Sensor;
        Sensor waterLevel = sensors[2].Sensor;

        return
        [
            new(
            "FuelCombustion",
            new FuelCombustion(),
            boilerTemperature,
            "thermalStation/1/actuators/fuelCombustion/state",
            onLevel: 1,
            offLevel: 80,
            onWhenAbove: false),

        new(
            "WaterFlowValve",
            new WaterFlowValve(),
            waterLevel,
            "thermalStation/1/actuators/waterFlowValve/state",
            onLevel: 2,
            offLevel: 70,
            onWhenAbove: false)
        ];
    }

    private static async Task RunSimulationAsync(
        IMqttClient mqttClient,
        SensorConfiguration[] sensors,
        ActuatorConfiguration[] actuators)
    {
        for (int iteration = 1; ; iteration++)
        {
            Console.WriteLine($"Iteration {iteration}.");

            foreach (var sensor in sensors)
            {
                await PublishSensorReadingAsync(mqttClient, sensor);
            }

            foreach (var actuator in actuators)
            {
                await UpdateActuatorAsync(mqttClient, actuator);
            }

            Console.WriteLine();
            await Task.Delay(TimeSpan.FromSeconds(2));
        }
    }

    private static async Task PublishSensorReadingAsync(
        IMqttClient mqttClient,
        SensorConfiguration configuration)
    {
        configuration.Sensor.ReadValue();

        string payload = JsonSerializer.Serialize(new
        {
            value = Math.Round(configuration.Sensor.Value, 2),
            measuredAtUtc = DateTimeOffset.UtcNow
        });

        var message = new MqttApplicationMessageBuilder()
            .WithTopic(configuration.Topic)
            .WithPayload(payload)
            .Build();

        await mqttClient.PublishAsync(message);

        Console.WriteLine($"Published {configuration.Topic} -> {payload}");
    }

    private static async Task UpdateActuatorAsync(
        IMqttClient mqttClient,
        ActuatorConfiguration configuration)
    {
        string? reason = configuration.Evaluate();
        bool isOn = true;

        if (reason is null && configuration.PublishedState == isOn)
        {
            return;
        }

        string payload = JsonSerializer.Serialize(new
        {
            isOn = true,
            reason = reason ?? "initial state",
            changedAtUtc = DateTimeOffset.UtcNow
        });

        var message = new MqttApplicationMessageBuilder()
            .WithTopic(configuration.Topic)
            .WithPayload(payload)
            .WithRetainFlag()
            .Build();

        await mqttClient.PublishAsync(message);
        configuration.PublishedState = isOn;

        Console.WriteLine($"Published {configuration.Topic} -> {payload}");
    }
}