namespace Consumer.Models;

public class Order
{
    public int Id { get; set; }

    public string Product { get; set; } = string.Empty;

    public int Quantity { get; set; }
}