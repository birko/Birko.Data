using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using Birko.Data.Models;

namespace Birko.Data.Stores
{
    public abstract class AbstractStore<T> : IStore<T, ISettings>
         where T : Models.AbstractModel
    {
        public abstract long Count(Expression<Func<T, bool>> filter);
        public abstract void Delete(T data);
        public abstract void Destroy();
        public abstract void Init();
        public abstract void List(Expression<Func<T, bool>> filter, Action<T> listAction);
        public abstract void Save(T data, StoreDataDelegate<T> storeDelegate = null);
        public abstract void SetSettings(ISettings settings);
        public abstract void StoreChanges();
        public virtual long Count()
        {
            return Count(null);
        }

        public virtual void List(Action<T> action)
        {
            List(null, action);
        }
    }
}
