using CommonLib.Core.DataWork;
using Microsoft.EntityFrameworkCore.Storage;
using ModelCore.DataEntity;
using ModelCore.DataEntity;
using System;
using System.Collections.Concurrent;
using TaskCenter.Core.Interfaces;

namespace TaskCenter.Core.Services
{
    /// <summary>
    /// Unit of Work implementation
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;


        /// <inheritdoc />
        public UnitOfWork(
            ApplicationDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public ApplicationDbContext Context => _context;

        /// <inheritdoc />
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        /// <inheritdoc />
        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        /// <inheritdoc />
        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }

    }
}
