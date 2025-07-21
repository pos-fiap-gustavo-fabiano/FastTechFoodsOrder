using FastTechFoodsOrder.Application.Interfaces;
using FastTechFoodsOrder.Infra.Context;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace FastTechFoodsOrder.Infra.UnitOfWork
{
    public class MongoUnitOfWork : IUnitOfWork
    {
        private readonly IMongoClient _client;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MongoUnitOfWork> _logger;
        private const int MaxRetryAttempts = 3;
        private readonly TimeSpan[] RetryDelays = { TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(200), TimeSpan.FromMilliseconds(400) };

        public MongoUnitOfWork(IMongoClient client, ApplicationDbContext context, ILogger<MongoUnitOfWork> logger)
        {
            _client = client;
            _context = context;
            _logger = logger;
        }

        public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation)
        {
            for (int attempt = 0; attempt < MaxRetryAttempts; attempt++)
            {
                IClientSessionHandle? session = null;
                try
                {
                    session = await _client.StartSessionAsync();
                    
                    session.StartTransaction();
                    
                    var result = await operation();
                    
                    await session.CommitTransactionAsync();
                    return result;
                }
                catch (MongoCommandException ex) when (IsRetriableError(ex) && attempt < MaxRetryAttempts - 1)
                {
                    _logger.LogWarning("Retriable MongoDB error detected on attempt {Attempt}/{MaxAttempts}. Retrying after {Delay}ms. Error: {Error}", 
                        attempt + 1, MaxRetryAttempts, RetryDelays[attempt].TotalMilliseconds, ex.Message);
                    
                    if (session != null && session.IsInTransaction)
                    {
                        await session.AbortTransactionAsync();
                    }
                    
                    // Aguarda antes de tentar novamente
                    await Task.Delay(RetryDelays[attempt]);
                    continue;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Non-retriable error in transaction on attempt {Attempt}/{MaxAttempts}", 
                        attempt + 1, MaxRetryAttempts);
                    
                    if (session != null && session.IsInTransaction)
                    {
                        await session.AbortTransactionAsync();
                    }
                    throw;
                }
                finally
                {
                    session?.Dispose();
                }
            }
            
            // Nunca deveria chegar aqui, mas por segurança
            throw new InvalidOperationException("Failed to execute transaction after maximum retry attempts");
        }

        public async Task ExecuteInTransactionAsync(Func<Task> operation)
        {
            for (int attempt = 0; attempt < MaxRetryAttempts; attempt++)
            {
                IClientSessionHandle? session = null;
                try
                {
                    session = await _client.StartSessionAsync();
                    
                    session.StartTransaction();
                    
                    await operation();
                    
                    await session.CommitTransactionAsync();
                    return;
                }
                catch (MongoCommandException ex) when (IsRetriableError(ex) && attempt < MaxRetryAttempts - 1)
                {
                    _logger.LogWarning("Retriable MongoDB error detected on attempt {Attempt}/{MaxAttempts}. Retrying after {Delay}ms. Error: {Error}", 
                        attempt + 1, MaxRetryAttempts, RetryDelays[attempt].TotalMilliseconds, ex.Message);
                    
                    if (session != null && session.IsInTransaction)
                    {
                        await session.AbortTransactionAsync();
                    }
                    
                    // Aguarda antes de tentar novamente
                    await Task.Delay(RetryDelays[attempt]);
                    continue;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Non-retriable error in transaction on attempt {Attempt}/{MaxAttempts}", 
                        attempt + 1, MaxRetryAttempts);
                    
                    if (session != null && session.IsInTransaction)
                    {
                        await session.AbortTransactionAsync();
                    }
                    throw;
                }
                finally
                {
                    session?.Dispose();
                }
            }
        }

        // Método com sessão para repositórios que precisam
        public async Task<T> ExecuteInTransactionAsync<T>(Func<IClientSessionHandle, Task<T>> operation)
        {
            for (int attempt = 0; attempt < MaxRetryAttempts; attempt++)
            {
                IClientSessionHandle? session = null;
                try
                {
                    session = await _client.StartSessionAsync();
                    
                    session.StartTransaction();
                    
                    var result = await operation(session);
                    
                    await session.CommitTransactionAsync();
                    return result;
                }
                catch (MongoCommandException ex) when (IsRetriableError(ex) && attempt < MaxRetryAttempts - 1)
                {
                    _logger.LogWarning("Retriable MongoDB error detected on attempt {Attempt}/{MaxAttempts}. Retrying after {Delay}ms. Error: {Error}", 
                        attempt + 1, MaxRetryAttempts, RetryDelays[attempt].TotalMilliseconds, ex.Message);
                    
                    if (session != null && session.IsInTransaction)
                    {
                        await session.AbortTransactionAsync();
                    }
                    
                    // Aguarda antes de tentar novamente
                    await Task.Delay(RetryDelays[attempt]);
                    continue;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Non-retriable error in transaction on attempt {Attempt}/{MaxAttempts}", 
                        attempt + 1, MaxRetryAttempts);
                    
                    if (session != null && session.IsInTransaction)
                    {
                        await session.AbortTransactionAsync();
                    }
                    throw;
                }
                finally
                {
                    session?.Dispose();
                }
            }
            
            // Nunca deveria chegar aqui, mas por segurança
            throw new InvalidOperationException("Failed to execute transaction after maximum retry attempts");
        }
        
        public async Task ExecuteInTransactionAsync(Func<IClientSessionHandle, Task> operation)
        {
            for (int attempt = 0; attempt < MaxRetryAttempts; attempt++)
            {
                IClientSessionHandle? session = null;
                try
                {
                    session = await _client.StartSessionAsync();
                    
                    session.StartTransaction();
                    
                    await operation(session);
                    
                    await session.CommitTransactionAsync();
                    return;
                }
                catch (MongoCommandException ex) when (IsRetriableError(ex) && attempt < MaxRetryAttempts - 1)
                {
                    _logger.LogWarning("Retriable MongoDB error detected on attempt {Attempt}/{MaxAttempts}. Retrying after {Delay}ms. Error: {Error}", 
                        attempt + 1, MaxRetryAttempts, RetryDelays[attempt].TotalMilliseconds, ex.Message);
                    
                    if (session != null && session.IsInTransaction)
                    {
                        await session.AbortTransactionAsync();
                    }
                    
                    // Aguarda antes de tentar novamente
                    await Task.Delay(RetryDelays[attempt]);
                    continue;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Non-retriable error in transaction on attempt {Attempt}/{MaxAttempts}", 
                        attempt + 1, MaxRetryAttempts);
                    
                    if (session != null && session.IsInTransaction)
                    {
                        await session.AbortTransactionAsync();
                    }
                    throw;
                }
                finally
                {
                    session?.Dispose();
                }
            }
        }

        private static bool IsRetriableError(MongoCommandException ex)
        {
            // Código 112 = WriteConflict no MongoDB
            // Código 16 = OperationFailed (pode incluir API parameter mismatch)
            // Código 50 = MaxTimeMSExpired
            // Também verifica pela mensagem de erro
            return ex.Code == 112 || // WriteConflict
                   ex.Code == 16 ||  // OperationFailed 
                   ex.Code == 50 ||  // MaxTimeMSExpired
                   ex.Message.Contains("Write conflict") || 
                   ex.Message.Contains("yielding is disabled") ||
                   ex.Message.Contains("API parameter mismatch") ||
                   ex.Message.Contains("commitTransaction used params") ||
                   ex.Message.Contains("the transaction's first command used");
        }
    }
}
