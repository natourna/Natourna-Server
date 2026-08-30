namespace NatournaServer.Interfaces.Context
{
    /// <summary>Runs a multi-write operation atomically; all ContextManagers share the scoped DbContext, so one transaction covers them.</summary>
    public interface ITransactionManager
    {
        Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation);

        Task ExecuteInTransactionAsync(Func<Task> operation);
    }
}
