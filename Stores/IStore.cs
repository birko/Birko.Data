using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Birko.Data.Stores
{
    public delegate T StoreDataDelegate<T>(T data) where T : Models.AbstractModel;

    public interface IBaseStore
    {
        void Init();
        void Destroy();
    }

    public interface ISettingsStore<TSettings>
    {
        void SetSettings(TSettings settings);
    }

    public interface ICountStore<T>
         where T : Models.AbstractModel
    { 
        long Count(Expression<Func<T, bool>>? filter = null);
    }

    public interface IReadStore<T>
         where T : Models.AbstractModel
    {
        T? ReadOne(Expression<Func<T, bool>>? filter = null);
    }

    public interface IDeleteStore<T>
        where T : Models.AbstractModel
    {
        void Delete(T id);
    }

    public interface ICreateStore<T>
         where T : Models.AbstractModel
    {
        void Create(T data, StoreDataDelegate<T>? storeDelegate = null);
    }

    public interface IUpdateStore<T>
         where T : Models.AbstractModel
    {
        void Update(T data, StoreDataDelegate<T>? storeDelegate = null);
    }

    public interface IStore<T>
        : IBaseStore
        , ICountStore<T>
        , IReadStore<T>
        , ICreateStore<T>
        , IUpdateStore<T>
        , IDeleteStore<T>
        where T : Models.AbstractModel
    {
        T CreateInstance();
    }

    public interface ISettingsStore<T, TSettings>
        : IStore<T>
        , ISettingsStore<TSettings>
        where T : Models.AbstractModel
        where TSettings : ISettings
    {
    }
}
