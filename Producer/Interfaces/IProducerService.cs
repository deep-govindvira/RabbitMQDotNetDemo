namespace Producer.Interfaces;

public interface IProducerService
{
    Task PublishAsync<T>(T message);
}