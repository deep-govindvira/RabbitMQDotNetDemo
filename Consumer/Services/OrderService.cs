using Consumer.Interfaces;
using Consumer.Models;

namespace Consumer.Services;

public class OrderService : IOrderService
{
    private readonly ILogger<OrderService> _logger;

    public OrderService(ILogger<OrderService> logger)
    {
        _logger = logger;
    }

    public async Task ProcessAsync(Order order)
    {
        _logger.LogInformation("Processing Order");

        _logger.LogInformation("Id : {Id}", order.Id);

        _logger.LogInformation("Product : {Product}", order.Product);

        _logger.LogInformation("Quantity : {Quantity}", order.Quantity);

        // Simulate long running work

        await Task.Delay(3000);

        _logger.LogInformation("Order Completed");
    }
}