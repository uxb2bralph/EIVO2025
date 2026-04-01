using CommonLib.Core.Properties;
using CommonLib.Logger;
using CommonLib.Utility;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections;
//using System.Data.Linq;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

namespace CommonLib.DataAccess
{
    /// <summary>
    /// UserManager 的摘要描述。
    /// </summary>
    public partial class GenericEnitytFrameworkManager<T, TEntity> : GenericEnitytFrameworkManager<T>
        where T : DbContext, new()
        where TEntity : class, new()
    {
        protected internal TEntity? _entity;

        public GenericEnitytFrameworkManager() : base() { }
        public GenericEnitytFrameworkManager(GenericEnitytFrameworkManager<T> mgr) : base(mgr) { }


        public TEntity? DataEntity
        {
            get
            {
                return _entity;
            }
        }

        public DbSet<TEntity> EntityList
        {
            get
            {
                return _db.Set<TEntity>();
            }
        }


        protected TEntity? instantiateData(IQueryable<TEntity> values)
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
            _db.Set<TEntity>().Remove(_entity);
            _db.SaveChanges();
            _entity = default(TEntity);
        }

        public TEntity DeleteAny(Expression<Func<TEntity, bool>> predicate)
        {
            return DeleteAny<TEntity>(predicate);
        }

        public TEntity DeleteAnyOnSubmit(Expression<Func<TEntity, bool>> predicate)
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

    public class GenericEnitytFrameworkManager<T> : IDisposable
                where T : DbContext, new()
    {
        protected internal T _db;
        protected internal bool _isInstance = true;

        private bool _bDisposed = false;
        private SqlLogger? _logWriter;

        public GenericEnitytFrameworkManager(T db)
        {
            //
            // TODO: 在此加入建構函式的程式碼
            //
            if (db != null)
            {
                _db = db;
                _isInstance = false;
                // 由外部傳入的 DbContext 不需要 Dispose
            }
            else
            {
                Initialize(null);
            }

            if(_db == null)
            {
                throw new Exception("DbContext cannot be null.");
            }   
        }

        public GenericEnitytFrameworkManager()
        {
            Initialize(null);
            if (_db == null)
            {
                throw new Exception("DbContext cannot be null.");
            }
        }


        public GenericEnitytFrameworkManager(GenericEnitytFrameworkManager<T>? mgr)
        {
            if (mgr != null)
            {
                _db = mgr.DataContext;
                _isInstance = false;
            }
            else
            {
                Initialize(null);
            }

            if (_db == null)
            {
                throw new Exception("DbContext cannot be null.");
            }
        }

        public GenericEnitytFrameworkManager(TextWriter? log)
        {
            Initialize(log);
            if (_db == null)
            {
                throw new Exception("DbContext cannot be null.");
            }
        }

        private void Initialize(TextWriter? log)
        {
            if (AppSettings.Default.SqlLog)
            {
                _logWriter = new SqlLogger { IgnoreSelect = AppSettings.Default.SqlLogIgnoreSelect };
            }

            log ??= _logWriter;

            if (log == null)
            {
                _db = new T();
            }
            else
            {
                var options = new DbContextOptionsBuilder<T>()
                        .LogTo(log.WriteLine) // 直接寫到 log
                        .EnableSensitiveDataLogging()
                        .Options;

                Type type = typeof(T);
                var assembly = type.Assembly;
                if (assembly != null)
                {
                    _db = assembly.CreateInstance(type.FullName!, false, System.Reflection.BindingFlags.CreateInstance, null,
                        new Object[] { options }, null, null) as T;
                }
            }
        }

        public IDbConnection Connection
        {
            get { return _db.Database.GetDbConnection(); }
        }

        public T DataContext
        {
            get { return _db; }
        }

        //public GenericEnitytFrameworkManager<U> BridgeManager<U>()
        //    where U : DbContext, new()
        //{
        //    return new GenericEnitytFrameworkManager<U>(_db.Database.GetDbConnection());
        //}

        public void SubmitChanges()
        {
            _db.SaveChanges();
        }


        public IDbContextTransaction EnterTransaction()
        {
            if (_db.Database.GetDbConnection().State != ConnectionState.Open)
            {
                _db.Database.GetDbConnection().Open();
            }

            return _db.Database.BeginTransaction();
        }
        public IDbContextTransaction EnterTransaction(IsolationLevel isolationLevel)
        {
            if (_db.Database.GetDbConnection().State != ConnectionState.Open)
            {
                _db.Database.GetDbConnection().Open();
            }
            return _db.Database.BeginTransaction(isolationLevel);
        }

        public DbSet<TTable> GetTable<TTable>() where TTable : class
        {
            return _db.Set<TTable>();
        }

        public DbCommand GetCommand(string commandText, params Object[] parameters)
        {
            DbCommand command = _db.Database.GetDbConnection().CreateCommand();
            command.CommandText = commandText;
            if (parameters != null && parameters.Length > 0)
            {
                foreach (var param in parameters)
                {
                    var dbParam = command.CreateParameter();
                    dbParam.Value = param;
                    command.Parameters.Add(dbParam);
                }
            }
            return command;
        }
        public IEnumerable<TResult> ExecuteQuery<TResult>(string query, params Object[] parameters)
            where TResult : class, new()
        {
            return _db.Set<TResult>().FromSqlRaw(query, parameters);
        }

        public int ExecuteCommand(string command, params Object[] parameters)
        {
            return _db.Database.ExecuteSqlRaw(command, parameters);
        }

        public TSource? DeleteAnyOnSubmit<TSource>(Expression<Func<TSource, bool>> predicate) where TSource : class, new()
        {
            var table = _db.Set<TSource>();
            TSource? item = table?.Where(predicate).FirstOrDefault();
            if (item != null)
            {
                table!.Remove(item);
            }
            return item;
        }

        public IEnumerable<TSource> DeleteAllOnSubmit<TSource>(Expression<Func<TSource, bool>> predicate) where TSource : class, new()
        {
            var table = _db.Set<TSource>();
            IEnumerable<TSource> items = table.Where(predicate);
            table.RemoveRange(items);
            return items;
        }

        public IEnumerable<TSource> DeleteAll<TSource>(Expression<Func<TSource, bool>> predicate) where TSource : class, new()
        {
            IEnumerable<TSource> items = DeleteAllOnSubmit<TSource>(predicate);
            _db.SaveChanges();
            return items;
        }


        public TSource? DeleteAny<TSource>(Expression<Func<TSource, bool>> predicate) where TSource : class, new()
        {
            TSource? item = DeleteAnyOnSubmit<TSource>(predicate);
            if (item != null)
            {
                _db.SaveChanges();
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
                        _db.Dispose();
                        _logWriter?.Dispose();
                    }
                }

                _bDisposed = true;
            }
        }

        ~GenericEnitytFrameworkManager()
        {
            Dispose(false);
        }

        #endregion

    }

}
