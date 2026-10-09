using AutomatedThermalStationSimulator;
using AutomatedThermalStationSimulator.Mqtt;
using Models;
using MQTTnet;
using System.Text.Json;

public class Program
{
    public static async Task Main()
    {
        SensorConfiguration[] sensors = CreateSensors();

        var factory = new MqttClientFactory();
        using var mqttClient = factory.CreateMqttClient();

        var options = new MqttClientOptionsBuilder()
            .WithTcpServer("127.0.0.1", 1883)
            .Build();

        await mqttClient.ConnectAsync(options);
        Console.WriteLine("Connected to MQTT broker.");

        await RunSimulationAsync(mqttClient, sensors);

        await mqttClient.DisconnectAsync();
    }

    private static SensorConfiguration[] CreateSensors()
    {
        return
            [
            new (
                new ThermalSensor("Outside temperature"),
                "thermalStation/1/sensors/outside/thermal",
                "C"),
            new (
                new SteamSensor("Outside steam"),
                "thermalStation/1/sensors/outside/steam",
                "bar"),
            new (
                new VoltageOutputSensor("Inside voltage"),
                "thermalStation/1/sensors/inside/voltage",
                "V"),
            new (
                new CurrentOutputSensor("Inside current"),
                "thermalStation/1/sensors/inside/current",
                "A"),
            ];
    }

    private static async Task RunSimulationAsync(
        IMqttClient mqttClient,
        SensorConfiguration[] sensors)
    {
        for (int iteration = 1; iteration <= 10; iteration++)
        {
            Console.WriteLine($"Iteration {iteration}.");


            foreach (var sensor in sensors)
            {
                await PublichSensorReadingAsync(mqttClient, sensor);
            }

            Console.WriteLine();
            await Task.Delay(TimeSpan.FromSeconds(2));
        }
    }

    private static async Task PublichSensorReadingAsync(
        IMqttClient mqttClient,
        SensorConfiguration configuration)
    {
        configuration.Sensor.ReadValue();

        string payload = JsonSerializer.Serialize(new
        {
            value = Math.Round(configuration.Sensor.Value, 2),
            unit = configuration.Unit,
            measuredAtUtc = DateTimeOffset.UtcNow
        });

        var message = new MqttApplicationMessageBuilder()
            .WithTopic(configuration.Topic)
            .WithPayload(payload)
            .Build();

        await mqttClient.PublishAsync(message);

        Console.WriteLine($"Published {configuration.Topic} -> {payload} ");
    }
}