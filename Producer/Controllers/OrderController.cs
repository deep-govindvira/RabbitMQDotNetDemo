using Microsoft.AspNetCore.Mvc;
using Producer.Models;
using Producer.Interfaces;

namespace Producer.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IProducerService _producerService;

    public OrdersController(IProducerService producerService)
    {
        _producerService = producerService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder(Order order)
    {
        await _producerService.PublishAsync(order);

        return Ok(new
        {
            Message = "Order sent successfully."
        });
    }
}