using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Birko.Data.Repository
{
    public abstract class AbstractRepository<TViewModel, TModel> : IRepository<TViewModel>
        where TModel:Model.AbstractModel, Model.ILoadable<TViewModel>
        where TViewModel:Model.ILoadable<TModel>
    {
        protected string _path = null;
        protected Store.IStore<TModel> _store;

        public virtual TViewModel Create(TViewModel data, ProcessDataDelegate processDelegate = null)
        {
            if (_store != null && data != null)
            {
                TModel item = (TModel)Activator.CreateInstance(typeof(TModel), new object[] { });
                item.LoadFrom(data);
                _store.Save(item, (x) => processDelegate?.Invoke(x));
                StoreChanges();
                data.LoadFrom(item);
            }
            return data;
        }

        public virtual TViewModel Delete(Guid Id)
        {
            if (_store != null && _store.Count(x => x.Guid == Id) > 0)
            {
                TViewModel result = (TViewModel)Activator.CreateInstance(typeof(TViewModel), new object[] { });
                _store.List(x => x.Guid == Id, (item) =>
                {
                    _store.Delete(item);
                    result.LoadFrom(item);
                });
                StoreChanges();
                return result;
            }
            return default(TViewModel);
        }

        public virtual long Count()
        {
            return (_store != null ) ?_store.Count() : 0;
        }

        public virtual TViewModel Read(Guid Id)
        {
            if (_store != null && _store.Count(x => x.Guid.Value == Id) > 0)
            {
                TViewModel result = (TViewModel)Activator.CreateInstance(typeof(TViewModel), new object[] { });
                _store.List(x => x.Guid == Id, (item) =>
                {
                    result.LoadFrom(item);
                });

                return result;
            }
            return default(TViewModel);
        }

        public virtual void Read(Action<TViewModel> readAction)
        {
            if (_store != null && _store.Count() > 0 && readAction != null)
            {
                _store.List((item) =>
                {
                    TViewModel result = (TViewModel)Activator.CreateInstance(typeof(TViewModel), new object[] { });
                    result.LoadFrom(item);
                    readAction?.Invoke(result);
                });
            }
        }

        public TViewModel Update(Guid Id, TViewModel data, ProcessDataDelegate processDelegate = null)
        {
            if (_store != null)
            {
                TModel item = (TModel)Activator.CreateInstance(typeof(TModel), new object[] { });
                item.LoadFrom(data);
                _store.Save(item, (x) => processDelegate?.Invoke(x));
                StoreChanges();
                data.LoadFrom(item);
            }
            return data;
        }

        public void StoreChanges()
        {
            _store?.StoreChanges();
        }
    }
}
