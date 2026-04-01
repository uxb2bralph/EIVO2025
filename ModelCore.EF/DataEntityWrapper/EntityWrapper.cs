using System;
using System.Collections.Generic;
using System.Text;

namespace ModelCore.DataEntityWrapper
{
    public abstract class EntityWrapper<T>
        where T : class
    {
        public T Entity { get; private set; }
        public EntityWrapper(T entity)
        {
            Entity = entity;
        }

    }
}
