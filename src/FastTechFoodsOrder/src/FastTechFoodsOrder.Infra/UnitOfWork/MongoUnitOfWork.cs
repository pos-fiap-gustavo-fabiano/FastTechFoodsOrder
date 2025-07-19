using FastTechFoodsOrder.Application.Interfaces;
using FastTechFoodsOrder.Infra.Context;
using MongoDB.Driver;

namespace FastTechFoodsOrder.Infra.UnitOfWork
{
    public class MongoUnitOfWork : IUnitOfWork
    {
        private readonly IMongoClient _client;
        private readonly ApplicationDbContext _context;

        public MongoUnitOfWork(IMongoClient client, ApplicationDbContext context)
        {
            _client = client;
            _context = context;
        }

        public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation)
        {
            // MongoDB Atlas suporta transações!
            using var session = await _client.StartSessionAsync();
            
            try
            {
                session.StartTransaction();
                
                var result = await operation();
                
                await session.CommitTransactionAsync();
                return result;
            }
            catch (Exception)
            {
                await session.AbortTransactionAsync();
                throw;
            }
        }

        public async Task ExecuteInTransactionAsync(Func<Task> operation)
        {
            using var session = await _client.StartSessionAsync();
            
            try
            {
                session.StartTransaction();
                
                await operation();
                
                await session.CommitTransactionAsync();
            }
            catch (Exception)
            {
                await session.AbortTransactionAsync();
                throw;
            }
        }

        // Método com sessão para repositórios que precisam
        public async Task<T> ExecuteInTransactionAsync<T>(Func<IClientSessionHandle, Task<T>> operation)
        {
            using var session = await _client.StartSessionAsync();
            
            try
            {
                session.StartTransaction();
                
                var result = await operation(session);
                
                await session.CommitTransactionAsync();
                return result;
            }
            catch (Exception)
            {
                await session.AbortTransactionAsync();
                throw;
            }
        }
        
        public async Task ExecuteInTransactionAsync(Func<IClientSessionHandle, Task> operation)
        {
            using var session = await _client.StartSessionAsync();
            
            try
            {
                session.StartTransaction();
                
                await operation(session);
                
                await session.CommitTransactionAsync();
            }
            catch (Exception)
            {
                await session.AbortTransactionAsync();
                throw;
            }
        }
    }
}
