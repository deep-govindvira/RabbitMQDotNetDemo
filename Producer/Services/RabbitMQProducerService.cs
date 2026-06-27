using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using Producer.Interfaces;
using Producer.Configuration;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using Serilog.Context;

namespace Producer.Services;

public class RabbitMQProducerService : IProducerService
{
    private readonly RabbitMQConfiguration _rabbitMQConfiguration;
    private readonly ILogger<RabbitMQProducerService> _logger;

    public RabbitMQProducerService(IOptions<RabbitMQConfiguration> options, ILogger<RabbitMQProducerService> logger)
    {
        _rabbitMQConfiguration = options.Value;
        _logger = logger;
    }

    public async Task PublishAsync<T>(T message)
    {
        var factory = new ConnectionFactory
        {
            HostName = _rabbitMQConfiguration.HostName,
            Port = _rabbitMQConfiguration.Port,
            UserName = _rabbitMQConfiguration.UserName,
            Password = _rabbitMQConfiguration.Password
        };

        var connection = await factory.CreateConnectionAsync();

        var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            queue: _rabbitMQConfiguration.QueueName,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var body = Encoding.UTF8.GetBytes(
            JsonSerializer.Serialize(message));

        var correlationId = Guid.NewGuid().ToString();

        var properties = new BasicProperties
        {
            Headers = new Dictionary<string, object?>
            {
                { "CorrelationId", correlationId }
            }
        };

        await channel.BasicPublishAsync(
            exchange: "",
            routingKey: _rabbitMQConfiguration.QueueName,
            mandatory: false,
            body: body,
            basicProperties: properties);

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            _logger.LogInformation(
                "Published {@Message} with Id {Id} to {Queue} queue",
                message,
                message?.GetType().GetProperty("Id")?.GetValue(message),
                _rabbitMQConfiguration.QueueName);

        }
    }
}