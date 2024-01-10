using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using Birko.Data.Models;

namespace Birko.Data.Stores
{
    public abstract class AbstractStore<T> : IStore<T>
         where T : Models.AbstractModel
    {
        public abstract long Count(Expression<Func<T, bool>>? filter = null);
        public abstract void Delete(T data);
        public abstract void Destroy();
        public abstract void Init();
        public abstract T? ReadOne(Expression<Func<T, bool>>? filter = null);
        public abstract void Create(T data, StoreDataDelegate<T>? storeDelegate = null);
        public abstract void Update(T data, StoreDataDelegate<T>? storeDelegate = null);

        public virtual void Save(T data, StoreDataDelegate<T> storeDelegate = null)
        {
            if (data == null)
            {
                return;
            }
            if (data.Guid == null || data.Guid == Guid.Empty)
            {
                Create(data, storeDelegate);
            }
            else
            {
                Update(data, storeDelegate);
            }
        }

        public T CreateInstance()
        {
            return (T)Activator.CreateInstance(typeof(T), new object[] { });
        }
    }
}
