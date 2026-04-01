using CommonLib.Core.DataWork;
using Microsoft.EntityFrameworkCore;
using ModelCore.DataEntity;
using System;
using System.Linq;
using System.Linq.Expressions;
using TaskCenter.Core.Interfaces;

namespace TaskCenter.Core.Services
{
    /// <summary>
    /// Generic repository implementation using GenericDbContext<ApplicationDbContext>.
    /// </summary>
    public class Repository<T> : GenericEntityRepository<ApplicationDbContext, T>, IRepository<T>
        where T : class, new()
    {
        /// <inheritdoc />
        protected readonly ApplicationDbContext _context;
        /// <inheritdoc />
        protected readonly DbSet<T> _dbSet;

        /// <inheritdoc />
        public Repository(ApplicationDbContext context) : base(new GenericDbContext<ApplicationDbContext>(context))
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        /// <inheritdoc />
        public virtual async Task<T?> GetByIdAsync(object id)
        {
            return await _dbSet.FindAsync(id);
        }

        /// <inheritdoc />
        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.AsNoTracking().ToListAsync();
        }

        /// <inheritdoc />
        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).AsNoTracking().ToListAsync();
        }

        /// <inheritdoc />
        public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

        /// <inheritdoc />
        public virtual async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        /// <inheritdoc />
        public virtual async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        /// <inheritdoc />
        public virtual void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        /// <inheritdoc />
        public virtual void UpdateRange(IEnumerable<T> entities)
        {
            _dbSet.UpdateRange(entities);
        }

        /// <inheritdoc />
        public virtual void Remove(T entity)
        {
            _dbSet.Remove(entity);
        }

        /// <inheritdoc />
        public virtual void RemoveRange(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
        }

        /// <inheritdoc />
        public virtual async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<T, bool>>? predicate = null)
        {
            var query = _dbSet.AsNoTracking();

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        /// <inheritdoc/>
        public virtual async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            IQueryable<T> query)
        {
            query = query.AsNoTracking();

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        /// <inheritdoc/>
        public DbSet<T> DbSet => _dbSet;

        /// <inheritdoc/>
        public async Task<IEnumerable<T>> GetQueryItemsAsync(Func<IQueryable<T>, IQueryable<T>> query)
        {
            return await query(_dbSet).AsNoTracking().ToListAsync();
        }

        /// <inheritdoc />
        public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
        {
            var query = _dbSet.AsNoTracking();

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            return await query.CountAsync();
        }
    }
}
