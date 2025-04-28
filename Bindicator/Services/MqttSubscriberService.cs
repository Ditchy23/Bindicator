using Bindicator.Data;
using Bindicator.Models;
using Microsoft.AspNetCore.SignalR;
using MQTTnet;
using System.Buffers;
using System.Text;
using System.Text.Json;

namespace Bindicator.Services;

/// <summary>
/// Background service that subscribes to MQTT topics and processes incoming messages.
/// </summary>
public class MqttSubscriberService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHubContext<Bindicator.Hubs.BinStatusHub> _hubContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="MqttSubscriberService"/> class.
    /// </summary>
    /// <param name="scopeFactory">The service scope factory to create scopes for database operations.</param>
    /// <param name="hubContext">The SignalR hub context for sending real-time updates to clients.</param>
    public MqttSubscriberService(IServiceScopeFactory scopeFactory, IHubContext<Bindicator.Hubs.BinStatusHub> hubContext)
    {
        _scopeFactory = scopeFactory;
        _hubContext = hubContext;
    }

    /// <summary>
    /// Executes the background service. Connects to the MQTT broker, subscribes to topics, and processes incoming messages.
    /// </summary>
    /// <param name="stoppingToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the background service execution.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var mqttFactory = new MqttClientFactory();
                using var mqttClient = mqttFactory.CreateMqttClient();

                var mqttClientOptions = new MqttClientOptionsBuilder()
                    .WithTcpServer("c79e2ea5e65e40f6b79ba3a3aad7c19f.s1.eu.hivemq.cloud", 8883)
                    .WithCredentials("admin", "Password1")
                    .WithTlsOptions(tls =>
                    {
                        tls.UseTls();
                    })
                    .Build();

                mqttClient.ApplicationMessageReceivedAsync += async e =>
                {
                    var topic = e.ApplicationMessage.Topic;
                    var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload.ToArray());

                    // Log the received message for debugging
                    Console.WriteLine("=========================================");
                    Console.WriteLine("?? MQTT MESSAGE RECEIVED");
                    Console.WriteLine($"   Topic:   {topic}");
                    Console.WriteLine($"   Payload: {payload}");
                    Console.WriteLine("=========================================");

                    // Guard against empty payloads
                    if (string.IsNullOrWhiteSpace(payload))
                    {
                        Console.WriteLine("⚠️  Payload is empty. Skipping.");
                        return;
                    }

                    // Guard against empty topics
                    var parts = topic.Split('/');
                    if (parts.Length < 3)
                    {
                        Console.WriteLine("⚠️  Topic does not have enough parts. Skipping.");
                        return;
                    }

                    // Guard against invalid topic format
                    string postcode = parts[0];
                    string street = parts[1];
                    if (!int.TryParse(parts[2], out int binNumber))
                    {
                        Console.WriteLine("⚠️  BinNumber is not a valid integer. Skipping.");
                        return;
                    }

                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    bool dataSaved = false;

                    try
                    {
                        if (topic.EndsWith("Sensors/Current", StringComparison.OrdinalIgnoreCase))
                        {
                            var data = JsonSerializer.Deserialize<SensorData>(payload, new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });

                            if (data == null)
                            {
                                Console.WriteLine("⚠️  SensorData deserialization failed.");
                                return;
                            }

                            data.Postcode = postcode;
                            data.Street = street;
                            data.BinNumber = binNumber;
                            data.Timestamp = DateTime.UtcNow;

                            db.SensorReadings.Add(data);
                            Console.WriteLine("💾 SensorData added to DB context.");
                            dataSaved = true;
                        }
                        else if (topic.EndsWith("Environment/Current", StringComparison.OrdinalIgnoreCase))
                        {
                            var data = JsonSerializer.Deserialize<EnvironmentData>(payload, new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });

                            if (data == null)
                            {
                                Console.WriteLine("⚠️  EnvironmentData deserialization failed.");
                                return;
                            }

                            data.Postcode = postcode;
                            data.Street = street;
                            data.BinNumber = binNumber;
                            data.Timestamp = DateTime.UtcNow;

                            db.EnvironmentReadings.Add(data);
                            Console.WriteLine("💾 EnvironmentData added to DB context.");
                            dataSaved = true;
                        }
                        else
                        {
                            Console.WriteLine("⚠️  Topic not recognized as supported. Skipping.");
                            return;
                        }

                        if (dataSaved)
                        {
                            var changes = await db.SaveChangesAsync(stoppingToken);
                            Console.WriteLine($"✅ DB changes saved: {changes} row(s) affected.");

                            // 🔥 Always notify SignalR if anything saved
                            await _hubContext.Clients.All.SendAsync("ReceiveTrendUpdate", postcode, street, binNumber);
                            await _hubContext.Clients.All.SendAsync("ReceiveBinUpdate");

                            Console.WriteLine("🔔 SignalR notifications sent.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Error saving to DB or sending SignalR: {ex.Message}");
                    }
                };

                // Try to connect (may throw if broker is unreachable)
                await mqttClient.ConnectAsync(mqttClientOptions, stoppingToken);

                var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
                    .WithTopicFilter(f => f.WithTopic("#").WithAtLeastOnceQoS())
                    .Build();

                await mqttClient.SubscribeAsync(mqttSubscribeOptions, stoppingToken);

                Console.WriteLine("✅ #");

                // Wait until cancelled (if connection drops, catch below will handle)
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"[MQTT] Error: {ex.Message}");

                // Wait before retrying
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }
}