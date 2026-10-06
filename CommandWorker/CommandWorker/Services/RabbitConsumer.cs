using System.Text;
using System.Text.Json;
using CommandWorker.Data;
using CommandWorker.Models;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;



namespace CommandWorker.Services;

public class RabbitConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ImportantLogService _importantLogs;
    private readonly ILogger<RabbitConsumer> _logger;

    public RabbitConsumer(
        IServiceScopeFactory scopeFactory,
        ImportantLogService importantLogs,
        ILogger<RabbitConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _importantLogs = importantLogs;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = "localhost"
            };

            _logger.LogInformation("Connecting to RabbitMQ");

            var connection = await factory.CreateConnectionAsync();
            var channel = await connection.CreateChannelAsync();

            _logger.LogInformation("Connected to RabbitMQ");

            string[] queues =
            {
                 "north.alerts", "center.alerts",
                 "south.alerts", "overseas.alerts"
            };

            foreach (var queue in queues)
            {
                await channel.QueueDeclareAsync(
                    queue: queue,
                    durable: true,
                    exclusive: false,
                    autoDelete: false
                );

                var consumer = new AsyncEventingBasicConsumer(channel);

                consumer.ReceivedAsync += async (_, ea) =>
                {
                    await HandleMessageAsync(channel, ea, stoppingToken);
                };

                await channel.BasicConsumeAsync(
                    queue: queue,
                    autoAck: false,
                    consumer: consumer
                );

                _logger.LogInformation("Listening to RabbitMQ queue {Queue}", queue);
            }

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (TaskCanceledException)
        {
            _logger.LogInformation("Rabbit consumer stopping");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Rabbit consumer stopped because of an error");
            await _importantLogs.WriteAsync(
                "WorkerFatalError",
                message: ex.Message
            );
        }
    }

    private async Task HandleMessageAsync(
        IChannel channel,
        BasicDeliverEventArgs ea,
        CancellationToken stoppingToken)
    {
        try
        {
            var bytes = ea.Body.ToArray();
            var json = Encoding.UTF8.GetString(bytes);

            _logger.LogInformation("RabbitMQ message received");

            Alert? alert;

            try
            {
                alert = JsonSerializer.Deserialize<Alert>(json);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Bad JSON received from RabbitMQ");
                await _importantLogs.WriteAsync(
                    "WorkerRejectedMessage",
                    message: "Bad JSON"
                );

                await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                return;
            }

            if (!IsValid(alert))
            {
                _logger.LogWarning("Invalid alert received from RabbitMQ");
                await _importantLogs.WriteAsync(
                    "WorkerRejectedMessage",
                    alert?.AlertId,
                    alert?.Command,
                    "Invalid alert"
                );

                await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                return;
            }

            _logger.LogInformation(
                "Alert {AlertId} deserialized for {Command}",
                alert!.AlertId,
                alert.Command
            );

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            alert.ReceivedAt = DateTime.UtcNow;

            var saved = await SaveToCommandTableAsync(db, alert, stoppingToken);

            if (!saved)
            {
                _logger.LogWarning(
                    "Alert {AlertId} already exists in MySQL table for {Command}",
                    alert.AlertId,
                    alert.Command
                );

                await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                return;
            }

            _logger.LogInformation(
                "Alert {AlertId} saved to MySQL table for {Command}",
                alert.AlertId,
                alert.Command
            );

            await _importantLogs.WriteAsync(
                "AlertSavedToMySql",
                alert.AlertId,
                alert.Command,
                $"Alert was saved to {alert.Command}_alerts"
            );

            await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed processing RabbitMQ message");

            await _importantLogs.WriteAsync(
                "WorkerError",
                message: ex.Message
            );

            await channel.BasicNackAsync(
                ea.DeliveryTag,
                multiple: false,
                requeue: false
            );
        }
    }

    private static async Task<bool> SaveToCommandTableAsync(
        AppDbContext db,
        Alert alert,
        CancellationToken stoppingToken)
    {
        switch (alert.Command.ToUpperInvariant())
        {
            case "NORTH":
                if (await db.NorthAlerts.AnyAsync(x => x.AlertId == alert.AlertId, stoppingToken))
                    return false;

                await db.NorthAlerts.AddAsync(new NorthAlert
                {
                    AlertId = alert.AlertId,
                    Source = alert.Source,
                    Title = alert.Title,
                    Content = alert.Content,
                    Priority = alert.Priority,
                    Classification = alert.Classification,
                    Lat = alert.Lat,
                    Lon = alert.Lon,
                    Timestamp = alert.Timestamp,
                    Status = alert.Status,
                    Command = alert.Command,
                    ReceivedAt = alert.ReceivedAt
                }, stoppingToken);
                break;

            case "CENTER":
                if (await db.CenterAlerts.AnyAsync(x => x.AlertId == alert.AlertId, stoppingToken))
                    return false;

                await db.CenterAlerts.AddAsync(new CenterAlert
                {
                    AlertId = alert.AlertId,
                    Source = alert.Source,
                    Title = alert.Title,
                    Content = alert.Content,
                    Priority = alert.Priority,
                    Classification = alert.Classification,
                    Lat = alert.Lat,
                    Lon = alert.Lon,
                    Timestamp = alert.Timestamp,
                    Status = alert.Status,
                    Command = alert.Command,
                    ReceivedAt = alert.ReceivedAt
                }, stoppingToken);
                break;

            case "SOUTH":
                if (await db.SouthAlerts.AnyAsync(x => x.AlertId == alert.AlertId, stoppingToken))
                    return false;

                await db.SouthAlerts.AddAsync(new SouthAlert
                {
                    AlertId = alert.AlertId,
                    Source = alert.Source,
                    Title = alert.Title,
                    Content = alert.Content,
                    Priority = alert.Priority,
                    Classification = alert.Classification,
                    Lat = alert.Lat,
                    Lon = alert.Lon,
                    Timestamp = alert.Timestamp,
                    Status = alert.Status,
                    Command = alert.Command,
                    ReceivedAt = alert.ReceivedAt
                }, stoppingToken);
                break;

            case "OVERSEAS":
                if (await db.OverseasAlerts.AnyAsync(x => x.AlertId == alert.AlertId, stoppingToken))
                    return false;

                await db.OverseasAlerts.AddAsync(new OverseasAlert
                {
                    AlertId = alert.AlertId,
                    Source = alert.Source,
                    Title = alert.Title,
                    Content = alert.Content,
                    Priority = alert.Priority,
                    Classification = alert.Classification,
                    Lat = alert.Lat,
                    Lon = alert.Lon,
                    Timestamp = alert.Timestamp,
                    Status = alert.Status,
                    Command = alert.Command,
                    ReceivedAt = alert.ReceivedAt
                }, stoppingToken);
                break;

            default:
                throw new InvalidOperationException($"Unknown command: {alert.Command}");
        }

        await db.SaveChangesAsync(stoppingToken);
        return true;
    }

    private static bool IsValid(Alert? alert)
    {
        return alert != null
            && !string.IsNullOrWhiteSpace(alert.AlertId)
            && !string.IsNullOrWhiteSpace(alert.Command)
            && alert.Lat >= -90
            && alert.Lat <= 90
            && alert.Lon >= -180
            && alert.Lon <= 180;
    }
}
