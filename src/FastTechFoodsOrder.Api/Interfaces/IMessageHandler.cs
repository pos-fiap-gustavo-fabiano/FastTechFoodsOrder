using System.Diagnostics;

namespace FastTechFoodsOrder.Api.Interfaces
{
    public interface IMessageHandler<T>
    {
        Task HandleAsync(T message, Activity? activity = null);
    }
}
