using Confluent.Kafka;

namespace NotificationGate.Services;

public class KafkaService : IDisposable
{
    private readonly IProducer<Null, string> _producer;

    public KafkaService(string bootstrapServers)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = bootstrapServers
        };

        _producer = new ProducerBuilder<Null, string>(config).Build();
    }

    public async Task SendAsync(string json)
    {
        await _producer.ProduceAsync(
            "alerts",
            new Message<Null, string> { Value = json }
        );
    }

    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(5));
        _producer.Dispose();
    }
}
