using Consumer.Services;

namespace Consumer.Services;

public class Worker : BackgroundService
{
    private readonly RabbitMQConsumerService _rabbitMQConsumerService;

    public Worker(RabbitMQConsumerService rabbitMQConsumerService)
    {
        _rabbitMQConsumerService = rabbitMQConsumerService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _rabbitMQConsumerService.StartListeningAsync();

        return;
    }
}