using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;

using System.Linq.Expressions;
using System.Collections;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace CommonLib.Core.DataWork
{
    /// <summary>
    /// UserManager 的摘要描述。
    /// </summary>
    public partial class GenericEntityRepository<T, TEntity> : GenericDbContext<T>, IGenericEntityRepository<TEntity> 
        where T : DbContext, new()
        where TEntity : class, new()
    {
        protected internal TEntity? _entity;

        public GenericEntityRepository() : base() { }
        public GenericEntityRepository(GenericDbContext<T>? models) : base(models) { }


        public TEntity? DataEntity => _entity;

        public DbSet<TEntity> EntityList => _db!.Set<TEntity>();


        protected TEntity? InstantiateData(IQueryable<TEntity> values)
        {
            _entity = values.FirstOrDefault();
            return _entity;
        }

        public virtual TEntity CreateEntity()
        {
            _entity = new TEntity();
            EntityList.Add(_entity);
            return _entity;
        }

        public void DeleteEntity()
        {
            if (_entity != null)
            {
                _db!.Set<TEntity>().Remove(_entity);
                _db.SaveChanges();
            }
            _entity = default(TEntity);
        }

        public TEntity? DeleteAny(Expression<Func<TEntity, bool>> predicate)
        {
            return DeleteAny<TEntity>(predicate);
        }

        public TEntity? DeleteAnyOnSubmit(Expression<Func<TEntity, bool>> predicate)
        {
            return DeleteAnyOnSubmit<TEntity>(predicate);
        }

        public IEnumerable<TEntity> DeleteAll(Expression<Func<TEntity, bool>> predicate)
        {
            return DeleteAll<TEntity>(predicate);
        }

        public IEnumerable<TEntity> DeleteAllOnSubmit(Expression<Func<TEntity, bool>> predicate)
        {
            return DeleteAllOnSubmit<TEntity>(predicate);
        }
    }

    public class GenericDbContext<T> : IDisposable, IGenericDbContext<T> where T : DbContext, new()
    {
        protected internal T? _db;
        protected internal bool _isInstance = true;

        private bool _bDisposed = false;

        public GenericDbContext(T? db)
        {
            //
            // TODO: 在此加入建構函式的程式碼
            //
            if (db != null)
            {
                _db = db;
                _isInstance = false;
            }
            else
            {
                _db = new T();
                _isInstance = true;
            }
        }

        public GenericDbContext()
        {
            _db = new T();
            _isInstance = true;
        }

        public GenericDbContext(GenericDbContext<T>? mgr)
        {
            if (mgr != null)
            {
                _db = mgr.DataContext;
                _isInstance = false;
            }
            else
            {
                _db = new T();
            }
        }


        internal IDbConnection DbConnection => _db!.Database.GetDbConnection();

        public T DataContext => _db!;


        public void SubmitChanges()
        {
            _db!.SaveChanges();
        }

        public void SaveChanges()
        {
            _db!.SaveChanges();
        }

        public DbSet<TTable> GetTable<TTable>() where TTable : class
        {
            return _db!.Set<TTable>();
        }

        public DbCommand GetCommand(IQueryable query)
        {
            //var sqlCmd = _db.Database.GetDbConnection().CreateCommand();
            //sqlCmd.CommandText = query.ToQueryString();
            //return sqlCmd;
            return query.CreateDbCommand();
        }

        public IEnumerable<TResult> ExecuteQuery<TResult>(string query, params Object[] parameters)
            where TResult : class
        {
            return _db!.Set<TResult>().FromSqlRaw(query, parameters);
        }

        public DbConnection Connection => _db!.Database.GetDbConnection();

        public int ExecuteCommand(string command, params Object[] parameters)
        {
            return _db!.Database.ExecuteSqlRaw(command, parameters);
        }

        public TSource? DeleteAnyOnSubmit<TSource>(Expression<Func<TSource, bool>> predicate) where TSource : class, new()
        {
            var DbSet = _db!.Set<TSource>();
            TSource? item = DbSet?.Where(predicate).FirstOrDefault();
            if (item != null)
            {
                DbSet!.Remove(item);
            }
            return item;
        }

        public IEnumerable<TSource> DeleteAllOnSubmit<TSource>(Expression<Func<TSource, bool>> predicate) where TSource : class, new()
        {
            var DbSet = _db!.Set<TSource>();
            IQueryable<TSource> items = DbSet.Where(predicate);
            DbSet.RemoveRange(items);
            return items;
        }

        public IEnumerable<TSource> DeleteAll<TSource>(Expression<Func<TSource, bool>> predicate) where TSource : class, new()
        {
            IEnumerable<TSource> items = DeleteAllOnSubmit<TSource>(predicate);
            _db!.SaveChanges();
            return items;
        }


        public TSource? DeleteAny<TSource>(Expression<Func<TSource, bool>> predicate) where TSource : class, new()
        {
            TSource? item = DeleteAnyOnSubmit<TSource>(predicate);
            if (item != null)
            {
                _db!.SaveChanges();
            }
            return item;
        }

        #region IDisposable 成員

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_bDisposed)
            {
                if (disposing)
                {
                    if (_isInstance)
                    {
                        _db!.Dispose();
                    }
                }

                _bDisposed = true;
            }
        }

        ~GenericDbContext()
        {
            Dispose(false);
        }

        #endregion

    }

}
