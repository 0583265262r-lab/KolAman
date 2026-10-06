using Confluent.Kafka;
using Microsoft.Extensions.Logging;


namespace NotificationGate.Services;

public class KafkaService : IDisposable
{
    private readonly IProducer<Null, string> _producer;
    private readonly ILogger<KafkaService> _logger;

    public KafkaService(string bootstrapServers,ILogger<KafkaService> logger)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = bootstrapServers
        };
        _logger = logger;

        _producer = new ProducerBuilder<Null, string>(config).Build();
        _logger.LogInformation("Kafka producer created");
    }

    public async Task SendAsync(string json)
    {
        _logger.LogInformation("Sending alert to Kafka topic alerts");
        await _producer.ProduceAsync(
            "alerts",
            new Message<Null, string> { Value = json }
        );
        _logger.LogInformation("Alert sent to Kafka successfully");
    }

    public void Dispose()
    {
        _logger.LogInformation("Closing Kafka producer");
        _producer.Flush(TimeSpan.FromSeconds(5));
        _producer.Dispose();
    }
}
