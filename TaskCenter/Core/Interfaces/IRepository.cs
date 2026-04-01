using CommonLib.Core.DataWork;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace TaskCenter.Core.Interfaces
{
    /// <summary>
    /// Generic repository interface for basic CRUD and query operations.
    /// </summary>
    public interface IRepository<T> : IGenericEntityRepository<T>
        where T : class, new()
    {
        /// <summary>
        /// Gets the <see cref="DbSet{T}"/> for the entity type.
        /// </summary>
        DbSet<T> DbSet { get; }

        /// <summary>
        /// Get entity by ID asynchronously
        /// </summary>
        /// <param name="id">Entity ID</param>
        /// <returns>Entity or null</returns>
        Task<T?> GetByIdAsync(object id);

        /// <summary>
        /// Get all entities asynchronously
        /// </summary>
        /// <returns>Collection of entities</returns>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// Find entities by predicate asynchronously
        /// </summary>
        /// <param name="predicate">Search predicate</param>
        /// <returns>Collection of matching entities</returns>
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Get first entity matching predicate asynchronously
        /// </summary>
        /// <param name="predicate">Search predicate</param>
        /// <returns>First matching entity or null</returns>
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Add entity
        /// </summary>
        /// <param name="entity">Entity to add</param>
        Task AddAsync(T entity);

        /// <summary>
        /// Add multiple entities
        /// </summary>
        /// <param name="entities">Entities to add</param>
        Task AddRangeAsync(IEnumerable<T> entities);

        /// <summary>
        /// Update entity
        /// </summary>
        /// <param name="entity">Entity to update</param>
        void Update(T entity);

        /// <summary>
        /// Update multiple entities
        /// </summary>
        /// <param name="entities">Entities to update</param>
        void UpdateRange(IEnumerable<T> entities);

        /// <summary>
        /// Remove entity
        /// </summary>
        /// <param name="entity">Entity to remove</param>
        void Remove(T entity);

        /// <summary>
        /// Remove multiple entities
        /// </summary>
        /// <param name="entities">Entities to remove</param>
        void RemoveRange(IEnumerable<T> entities);

        /// <summary>
        /// Get paged results asynchronously
        /// </summary>
        /// <param name="pageNumber">Page number (1-based)</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="predicate">Optional filter predicate</param>
        /// <returns>Paged result</returns>
        Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<T, bool>>? predicate = null);

        /// <summary>
        /// Executes a custom query on the entity set asynchronously and returns the resulting items.
        /// </summary>
        /// <param name="query">A function that applies additional query logic to the entity set.</param>
        /// <returns>A collection of entities resulting from the query.</returns>
        Task<IEnumerable<T>> GetQueryItemsAsync(Func<IQueryable<T>, IQueryable<T>> query);

        /// <summary>
        /// Gets a paged result asynchronously from a custom query.
        /// </summary>
        /// <param name="pageNumber">Page number (1-based).</param>
        /// <param name="pageSize">Page size.</param>
        /// <param name="query">The query to apply paging to.</param>
        /// <returns>Paged result containing items and total count.</returns>
        Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, IQueryable<T> query);

        /// <summary>
        /// Count entities matching predicate asynchronously
        /// </summary>
        /// <param name="predicate">Optional filter predicate</param>
        /// <returns>Count of matching entities</returns>
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
    }
}
