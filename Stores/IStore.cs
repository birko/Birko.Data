using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Birko.Data.Stores
{
    public delegate T StoreDataDelegate<T>(T data) where T : Models.AbstractModel;

    public interface IBaseStore
    {
        long Count();
        void Init();
        void StoreChanges();
        void Destroy();
    }


    public interface ISettingsStore<TSettings> : IBaseStore
    {
        void SetSettings(TSettings settings);
    }

    public interface IStore<T, TSettings> : ISettingsStore<TSettings>
        where T : Models.AbstractModel
        where TSettings : ISettings
    {
        void List(Action<T> listAction);
        void List(Expression<Func<T, bool>> filter, Action<T> listAction, int? limit = null, int? offset = null);
        long Count(Expression<Func<T, bool>> filter);
        void Save(T data, StoreDataDelegate<T> storeDelegate = null);
        void Delete(T data);
    }
}
