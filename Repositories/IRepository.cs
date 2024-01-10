using Birko.Data.Filters;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;

namespace Birko.Data.Repositories
{
    public delegate TModel ProcessDataDelegate<TModel>(TModel data)
        where TModel : Models.AbstractModel;

    public interface IBaseRepository
    {
        void Destroy();
    }

    public interface ISettingsRepository<TSettings>
          where TSettings : Stores.ISettings
    {
        void SetSettings(TSettings settings);
    }

    public interface ICountRepository<TModel>
        where TModel : Models.AbstractModel
    {
        long Count(IRepositoryFilter<TModel>? filter = null);
    }

    public interface IReadRepository<T, TModel>
        where TModel : Models.AbstractModel
    {
        T? ReadOne(IRepositoryFilter<TModel>? filter = null);
    }

    public interface IDeleteRepository<T>
    {
        void Delete(T data);
    }

    public interface ICreateRepository<T, TModel>
        where TModel : Models.AbstractModel
    {
        void Create(T data, ProcessDataDelegate<TModel>? processDelegate = null);
    }

    public interface IUpdateRepository<T, TModel>
        where TModel : Models.AbstractModel
    {
        void Update(T data, ProcessDataDelegate<TModel>? processDelegate = null);
    }

    public interface IRepository<T, TModel>
        : IBaseRepository
        , ICountRepository<TModel>
        , IReadRepository<T, TModel>
        , ICreateRepository<T, TModel>
        , IUpdateRepository<T, TModel>
        , IDeleteRepository<T>

        where T : Models.ILoadable<TModel>
        where TModel : Models.AbstractModel, Models.ILoadable<T>
    {
        T CreateInstance();
        TModel CreateModelInstance();
    }


    public interface ISettingsRepository<T, TModel, TSettings>
        : ISettingsRepository<TSettings>
        , IRepository<T, TModel>

        where T : Models.ILoadable<TModel>
        where TModel : Models.AbstractModel, Models.ILoadable<T>
        where TSettings : Stores.ISettings
    {
    }
}
