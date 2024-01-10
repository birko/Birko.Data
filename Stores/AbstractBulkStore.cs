using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using Birko.Data.Models;

namespace Birko.Data.Stores
{
    public abstract class AbstractBulkStore<T> : AbstractStore<T>, IBulkStore<T>
         where T : Models.AbstractModel
    {
        public abstract IEnumerable<T> Read(Expression<Func<T, bool>>? filter = null, int? limit = null, int? offset = null);
        public abstract void Create(IEnumerable<T> data, StoreDataDelegate<T>? storeDelegate = null);
        public abstract void Delete(IEnumerable<T> data);
        public abstract void Update(IEnumerable<T> data, StoreDataDelegate<T>? storeDelegate = null);

        public virtual IEnumerable<T> Read()
        {
            foreach (var item in Read(null, null, null)) 
            {
                yield return item;
            }
        }
    }
}
