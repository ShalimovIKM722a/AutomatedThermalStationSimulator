using Models.Entities;
using MQTTnet;
using System.Text.Json;

namespace ThermalStationScada.Web.Services
{
    public class MqttListener : BackgroundService
    {
        private static readonly JsonSerializerOptions JsonOptions =
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        private readonly LiveValuesCache _cache;
        private readonly IConfiguration _config;
        private readonly ILogger<MqttListener> _logger;
        public MqttListener(LiveValuesCache cache, IConfiguration config,
        ILogger<MqttListener> logger)
        {
            _cache = cache;
            _config = config;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken
        stoppingToken)
        {
            MqttClientFactory factory = new MqttClientFactory();
            using IMqttClient client = factory.CreateMqttClient();
            client.ApplicationMessageReceivedAsync += OnMessageReceivedAsync;
            MqttClientOptions options = new MqttClientOptionsBuilder()
            .WithTcpServer(_config["Mqtt:Host"],
            int.Parse(_config["Mqtt:Port"]!))
            .Build();
            // якщо брокер ще не запущений, пробуємо знову кожні 5 секунд
            while (!client.IsConnected && !stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await client.ConnectAsync(options, stoppingToken);
                    _logger.LogInformation("Підключено до MQTT-брокера");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Брокер недоступний ({Message}), повтор через 5 с", ex.Message);

                    await Task.Delay(5000, stoppingToken);
                }
            }
            MqttClientSubscribeOptions subscribeOptions =
            factory.CreateSubscribeOptionsBuilder()
            .WithTopicFilter(_config["Mqtt:TopicFilter"]!)
            .Build();
            await client.SubscribeAsync(subscribeOptions, stoppingToken);
            _logger.LogInformation("Підписано на {Filter}",
            _config["Mqtt:TopicFilter"]);
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        private Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
        {
            try
            {
                string topic = e.ApplicationMessage.Topic;
                string json = e.ApplicationMessage.ConvertPayloadToString();
                MqttPayload? payload = JsonSerializer.Deserialize<MqttPayload>
                (json, JsonOptions);
                if (payload is not null)
                {
                    SensorReading reading = new SensorReading
                    {
                        Topic = topic,
                        Value = payload.Value,
                        Unit = payload.Unit,
                        MeasuredAtUtc = payload.MeasuredAtUtc
                    };
                    _cache.Update(reading);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Не вдалося обробити повідомлення");
            }
            return Task.CompletedTask;
        }

        private class MqttPayload
        {
            public double Value { get; set; }
            public string Unit { get; set; } = "";
            public DateTimeOffset MeasuredAtUtc { get; set; }
        }
    }
}