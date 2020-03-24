using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Birko.Data.Repositories
{
    public delegate TModel ProcessDataDelegate<TModel>(TModel data) where TModel: Models.AbstractModel;


    public interface IBaseRepository
    {

        long Count();
        void Destroy();
    }

    public interface IStoreRepository<TSettings> : IBaseRepository
          where TSettings : Stores.ISettings
    {
        void SetSettings(TSettings settings);
    }
    public interface IRepository<T, TModel, TSettings> : IStoreRepository<TSettings>
        where T: Models.ILoadable<TModel>
        where TModel : Models.AbstractModel, Models.ILoadable<T>
        where TSettings: Stores.ISettings
    {

        void Read(Action<T> readAction, int? limit = null, int? offset = null);
        void Read(Expression<Func<TModel, bool>> expr, Action<T> readAction, int? limit = null, int? offset = null);
        T Read(Guid Id);
        T Create(T data, ProcessDataDelegate<TModel> processDelegate = null);
        T Update(Guid Id, T data, ProcessDataDelegate<TModel> processDelegate = null);
        T Delete(Guid Id);
    }
}
