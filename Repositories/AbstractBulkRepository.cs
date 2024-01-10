using Birko.Data.Filters;
using Birko.Data.Stores;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Birko.Data.Repositories
{
    public abstract class AbstractBulkRepository<TViewModel, TModel> 
        : AbstractRepository<TViewModel, TModel>
        , IBulkRepository<TViewModel, TModel>
        where TModel:Models.AbstractModel, Models.ILoadable<TViewModel>
        where TViewModel:Models.ILoadable<TModel>
    {
        public AbstractBulkRepository() : base()
        {

        }

        public virtual IEnumerable<TViewModel> Read(IRepositoryFilter<TModel>? filter = null, int? limit = null, int? offset = null)
        {
            if (Store is not IBulkStore<TModel>)
            {
                throw new ArgumentException($"Store is not type of {typeof(IBulkStore<TModel>)}");
            }

            foreach (var item in ((IBulkStore<TModel>)Store).Read(filter?.Filter(), limit, offset))
            {
                yield return LoadInstance(item);
            }
        }

        public virtual void Create(IEnumerable<TViewModel> data, ProcessDataDelegate<TModel>? processDelegate = null)
        {
            if (Store is not IBulkStore<TModel>)
            {
                throw new ArgumentException($"Store is not type of {typeof(IBulkStore<TModel>)}");
            }

            (Store as IBulkStore<TModel>)?.Create(data.Select(x =>
            {
                TModel item = LoadModelInstance(x);
                processDelegate?.Invoke(item);
                return item;
            }));
        }


        public virtual void Update(IEnumerable<TViewModel> data, ProcessDataDelegate<TModel>? processDelegate = null)
        {
            if (Store is not IBulkStore<TModel>)
            {
                throw new ArgumentException($"Store is not type of {typeof(IBulkStore<TModel>)}");
            }
            (Store as IBulkStore<TModel>)?.Update(data.Select(x =>
            {
                TModel item = LoadModelInstance(x);
                processDelegate?.Invoke(item);
                return item;
            }));
        }

        public virtual void Delete(IEnumerable<TViewModel>data)
        {
            if (Store is not IBulkStore<TModel>)
            {
                throw new ArgumentException($"Store is not type of {typeof(IBulkStore<TModel>)}");
            }
            (Store as IBulkStore<TModel>)?.Delete(data.Select(LoadModelInstance));
        }
    }
}
