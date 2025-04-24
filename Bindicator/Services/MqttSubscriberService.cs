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
                    // ... [same as before, your message handling code] ...
                };

                // Try to connect (may throw if broker is unreachable)
                await mqttClient.ConnectAsync(mqttClientOptions, stoppingToken);

                var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
                    .WithTopicFilter(f => f.WithTopic("TS16/#").WithAtLeastOnceQoS())
                    .Build();

                await mqttClient.SubscribeAsync(mqttSubscribeOptions, stoppingToken);

                Console.WriteLine("✅ Subscribed to TS16/#");

                // Wait until cancelled (if connection drops, catch below will handle)
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"[MQTT] Error: {ex.Message}");

                // Wait a bit before retrying
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }

}