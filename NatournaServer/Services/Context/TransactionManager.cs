using NatournaServer.Data;
using NatournaServer.Interfaces.Context;

namespace NatournaServer.Services.Context
{
    public class TransactionManager : ITransactionManager
    {
        private readonly NatournaServerContext _context;
        private readonly ILogger<TransactionManager> _logger;

        public TransactionManager(NatournaServerContext context, ILogger<TransactionManager> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation)
        {
            // A transaction may already be open when operations compose (e.g. registration); join it instead of nesting
            if (_context.Database.CurrentTransaction != null)
            {
                return await operation();
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                T result = await operation();
                await transaction.CommitAsync();
                return result;
            }
            catch
            {
                await transaction.RollbackAsync();
                _logger.LogWarning("Transaction rolled back");
                throw;
            }
        }

        public async Task ExecuteInTransactionAsync(Func<Task> operation)
        {
            await ExecuteInTransactionAsync(async () =>
            {
                await operation();
                return true;
            });
        }
    }
}
