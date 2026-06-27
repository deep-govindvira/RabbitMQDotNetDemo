using System.Text;
using System.Text.Json;
using Consumer.Configuration;
using Consumer.Interfaces;
using Consumer.Models;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Consumer.Services;

public class RabbitMQConsumerService : IConsumerService
{
    private readonly RabbitMQConfiguration _rabbitMQConfiguration;
    private readonly IOrderService _orderService;
    private readonly ILogger<RabbitMQConsumerService> _logger;

    public RabbitMQConsumerService(
        IOptions<RabbitMQConfiguration> rabbitMQOptions,
        IOrderService orderService,
        ILogger<RabbitMQConsumerService> logger)
    {
        _rabbitMQConfiguration = rabbitMQOptions.Value;
        _orderService = orderService;
        _logger = logger;
    }
    public async Task StartListeningAsync()
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

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_, e) =>
        {

            if (e.BasicProperties.Headers != null)
{
    foreach (var header in e.BasicProperties.Headers)
    {
        var value = header.Value switch
        {
            byte[] bytes => Encoding.UTF8.GetString(bytes),
            _ => header.Value?.ToString()
        };

        _logger.LogInformation(
            "Header: {Key} = {Value}",
            header.Key,
            value);
    }
}
            var correlationId = GetHeader(e, "CorrelationId");

            using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
            {
                _logger.LogInformation("Consumed message");

                var json = Encoding.UTF8.GetString(e.Body.ToArray());

                var order = JsonSerializer.Deserialize<Order>(json);

                if (order != null)
                {
                    await _orderService.ProcessAsync(order);
                }
            }
        };

        await channel.BasicConsumeAsync(
            queue: _rabbitMQConfiguration.QueueName,
            autoAck: true,
            consumer: consumer);
    }

    private static string? GetHeader(BasicDeliverEventArgs e, string key)
    {
        if (e.BasicProperties.Headers == null)
            return null;

        if (!e.BasicProperties.Headers.TryGetValue(key, out var value))
            return null;

        return value switch
        {
            byte[] bytes => Encoding.UTF8.GetString(bytes),
            string str => str,
            _ => value?.ToString()
        };
    }
}