using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Birko.Data.Store
{
    public interface IStore<T> where T: Model.AbstractModel
    {
        IEnumerable<T> List();
        long Count();
        long Count(Expression<Func<T, bool>> filter);
        void Init();
        void Save(T data);
        void Delete(T data);
        void StoreChanges();
    }
}
