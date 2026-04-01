using ModelCore.DataEntity;
using System;

namespace TaskCenter.Core.Interfaces
{
    /// <summary>
    /// Unit of Work pattern interface for managing database transactions
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Database context
        /// </summary>
        ApplicationDbContext Context { get; }

        /// <summary>
        /// Save all changes asynchronously
        /// </summary>
        /// <returns>Number of affected records</returns>
        Task<int> SaveChangesAsync();

        /// <summary>
        /// Begin a database transaction
        /// </summary>
        /// <returns>Database transaction</returns>
        Task BeginTransactionAsync();

        /// <summary>
        /// Commit the current transaction
        /// </summary>
        Task CommitTransactionAsync();

        /// <summary>
        /// Rollback the current transaction
        /// </summary>
        Task RollbackTransactionAsync();
    }

}
