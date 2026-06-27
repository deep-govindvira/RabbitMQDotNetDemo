using Consumer.Models;

namespace Consumer.Interfaces;

public interface IOrderService
{
    Task ProcessAsync(Order order);
}