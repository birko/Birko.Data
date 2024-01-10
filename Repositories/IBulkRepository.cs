using Birko.Data.Filters;
using Birko.Data.Stores;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;

namespace Birko.Data.Repositories
{
    public interface IBulkDeleteRepository<T, TModel>
        where T : Models.ILoadable<TModel>
        where TModel : Models.AbstractModel
    {
        void Delete(IEnumerable<T> data);
    }

    public interface IBulkReadRepository<T, TModel>
        where T : Models.ILoadable<TModel>
        where TModel : Models.AbstractModel
    {
        IEnumerable<T> Read(IRepositoryFilter<TModel>? filter = null, int? limit = null, int? offset = null);
    }

    public interface IBulkCreateRepository<T, TModel>
        where T : Models.ILoadable<TModel>
        where TModel : Models.AbstractModel
    {
        void Create(IEnumerable<T> data, ProcessDataDelegate<TModel>? processDelegate = null);
    }

    public interface IBulkUpdateRepository<T, TModel>
        where T : Models.ILoadable<TModel>
        where TModel : Models.AbstractModel
    {
        void Update(IEnumerable<T> data, ProcessDataDelegate<TModel>? processDelegate = null);
    }

   public interface IBulkRepository<T, TModel>
        : IRepository<T,TModel>
        , IBulkReadRepository<T, TModel> 
        , IBulkCreateRepository<T, TModel>
        , IBulkUpdateRepository<T, TModel>
        , IBulkDeleteRepository<T, TModel>
        where T : Models.ILoadable<TModel>
        where TModel : Models.AbstractModel, Models.ILoadable<T>
    {
    }

    public interface ISettingsBulkRepository<T, TModel, TSettings>
        : IBulkRepository<T, TModel>
        , ISettingsRepository<TSettings>
        , ISettingsRepository<T, TModel, TSettings>
        where T : Models.ILoadable<TModel>
        where TModel : Models.AbstractModel, Models.ILoadable<T>
        where TSettings : Stores.ISettings
    { 
    }
}
