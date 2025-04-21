using System.Text;
using System.Text.Json;
using MQTTnet;
using Bindicator.Data;
using Bindicator.Models;
using System.Buffers;

namespace Bindicator.Services;

/// <summary>
/// Background service that subscribes to MQTT topics and processes incoming messages.
/// </summary>
public class MqttSubscriberService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="MqttSubscriberService"/> class.
    /// </summary>
    /// <param name="scopeFactory">The service scope factory to create scopes for database operations.</param>
    public MqttSubscriberService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    /// <summary>
    /// Executes the background service. Connects to the MQTT broker, subscribes to topics, and processes incoming messages.
    /// </summary>
    /// <param name="stoppingToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the background service execution.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Create a new MQTT client factory
        var mqttFactory = new MqttClientFactory();

        using var mqttClient = mqttFactory.CreateMqttClient();

        // Configure TLS options for secure connection
        var mqttClientOptions = new MqttClientOptionsBuilder()
            .WithTcpServer("c79e2ea5e65e40f6b79ba3a3aad7c19f.s1.eu.hivemq.cloud", 8883)
            .WithCredentials("admin", "Password1")
            .WithTlsOptions(tls =>
            {
                tls.UseTls();
            })
            .Build();

        // Set up event handler for when a message is received
        mqttClient.ApplicationMessageReceivedAsync += async e =>
        {
            var topic = e.ApplicationMessage.Topic;
            var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload.ToArray());

            Console.WriteLine($"📥 Topic: {topic}");
            Console.WriteLine($"🔹 Payload: {payload}");

            var parts = topic.Split('/');
            if (parts.Length < 3) return;

            string postcode = parts[0];
            string street = parts[1];
            if (!int.TryParse(parts[2], out int binNumber)) return;

            // Create a new scope for database operations
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            try
            {
                // Deserialize the payload based on the topic
                if (topic.EndsWith("Sensors/Current", StringComparison.OrdinalIgnoreCase))
                {
                    var data = JsonSerializer.Deserialize<SensorData>(payload, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });


                    if (data != null)
                    {
                        data.Postcode = postcode;
                        data.Street = street;
                        data.BinNumber = binNumber;
                        data.Timestamp = DateTime.UtcNow;
                        db.SensorReadings.Add(data);
                    }
                }
                else if (topic.EndsWith("Environment/Current",StringComparison.OrdinalIgnoreCase))
                {
                    var data = JsonSerializer.Deserialize<EnvironmentData>(payload, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (data != null)
                    {
                        data.Postcode = postcode;
                        data.Street = street;
                        data.BinNumber = binNumber;
                        data.Timestamp = DateTime.UtcNow;
                        db.EnvironmentReadings.Add(data);
                    }
                }

                await db.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error saving to DB: {ex.Message}");
            }
        };

        // Connect to the MQTT broker
        await mqttClient.ConnectAsync(mqttClientOptions, stoppingToken);

        // Log connection status
        var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
            .WithTopicFilter(f => f.WithTopic("TS16/#").WithAtLeastOnceQoS())
            .Build();

        // Subscribe to the topic
        var response = await mqttClient.SubscribeAsync(mqttSubscribeOptions, stoppingToken);

        Console.WriteLine("✅ Subscribed to TS16/#");

        // Keep service running
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}