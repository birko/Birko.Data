using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Birko.Data.Store
{
    public delegate T StoreDataDelegate<T>(T data) where T : Model.AbstractModel;
    public interface IStore<T> where T: Model.AbstractModel
    {
        void List(Action<T> listAction);
        void List(Expression<Func<T, bool>> filter, Action<T> listAction);
        long Count();
        long Count(Expression<Func<T, bool>> filter);
        void Init();
        void Save(T data, StoreDataDelegate<T> storeDelegate = null);
        void Delete(T data);
        void StoreChanges();
        void Destroy();
    }
}
