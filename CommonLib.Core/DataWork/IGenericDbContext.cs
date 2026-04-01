using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;

namespace CommonLib.Core.DataWork
{
    public interface IGenericDbContext<T> 
        where T : DbContext, new()
    {
        DbConnection Connection { get; }
        T DataContext { get; }

        IEnumerable<TSource> DeleteAll<TSource>(Expression<Func<TSource, bool>> predicate) where TSource : class, new();
        IEnumerable<TSource> DeleteAllOnSubmit<TSource>(Expression<Func<TSource, bool>> predicate) where TSource : class, new();
        TSource? DeleteAny<TSource>(Expression<Func<TSource, bool>> predicate) where TSource : class, new();
        TSource? DeleteAnyOnSubmit<TSource>(Expression<Func<TSource, bool>> predicate) where TSource : class, new();
        void Dispose();
        int ExecuteCommand(string command, params object[] parameters);
        IEnumerable<TResult> ExecuteQuery<TResult>(string query, params object[] parameters) where TResult : class;
        DbCommand GetCommand(IQueryable query);
        DbSet<TTable> GetTable<TTable>() where TTable : class;
        void SaveChanges();
        void SubmitChanges();

    }
}