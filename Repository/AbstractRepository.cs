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

        public virtual TViewModel Create(TViewModel data)
        {
            if (_store != null && data != null)
            {
                TModel item = (TModel)Activator.CreateInstance(typeof(TModel), new object[] { });
                item.LoadFrom(data);
                _store.Save(item);
                data.LoadFrom(item);
                _store.StoreChanges();
            }
            return data;
        }

        public virtual TViewModel Delete(Guid Id)
        {
            if (_store != null && _store.List() != null && _store.List().Any(x => x.Guid.Value == Id))
            {
                var item = _store.List().FirstOrDefault(x => x.Guid.Value == Id);
                _store.Delete(item);
                _store.StoreChanges();
                TViewModel result = (TViewModel)Activator.CreateInstance(typeof(TViewModel), new object[] { });
                result.LoadFrom(item);
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
            if (_store != null && _store.List() != null && _store.List().Any(x => x.Guid == Id))
            {
                var item = _store.List().FirstOrDefault(x => x.Guid == Id);
                TViewModel result = (TViewModel)Activator.CreateInstance(typeof(TViewModel), new object[] { });
                result.LoadFrom(item);
                return result;
            }
            return default(TViewModel);
        }

        public virtual IEnumerable<TViewModel> Read()
        {
            if (_store != null && _store.List() != null)
            {
                foreach (var item in _store.List())
                {
                    TViewModel result = (TViewModel)Activator.CreateInstance(typeof(TViewModel), new object[] { });
                    result.LoadFrom(item);
                    yield return result;
                }
            }
        }

        public TViewModel Update(Guid Id, TViewModel data)
        {
            if (_store != null)
            {
                TModel item = (TModel)Activator.CreateInstance(typeof(TModel), new object[] { });
                item.LoadFrom(data);
                _store.Save(item);
                _store.StoreChanges();
            }
            return data;
        }
    }
}
