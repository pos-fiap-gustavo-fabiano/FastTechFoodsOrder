using MongoDB.Driver;

namespace FastTechFoodsOrder.Application.Interfaces
{
    public interface IUnitOfWork
    {
        Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation);
        Task ExecuteInTransactionAsync(Func<Task> operation);
        
        // Novos métodos com sessão para repositórios
        Task<T> ExecuteInTransactionAsync<T>(Func<IClientSessionHandle, Task<T>> operation);
        Task ExecuteInTransactionAsync(Func<IClientSessionHandle, Task> operation);
    }
}
