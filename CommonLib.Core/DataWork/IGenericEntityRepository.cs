using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace CommonLib.Core.DataWork
{
    public interface IGenericEntityRepository<T> where T : class, new()
    {
        T? DataEntity { get; }
        DbSet<T> EntityList { get; }

        T CreateEntity();
        IEnumerable<T> DeleteAll(Expression<Func<T, bool>> predicate);
        IEnumerable<T> DeleteAllOnSubmit(Expression<Func<T, bool>> predicate);
        T? DeleteAny(Expression<Func<T, bool>> predicate);
        T? DeleteAnyOnSubmit(Expression<Func<T, bool>> predicate);
        void DeleteEntity();
    }
}