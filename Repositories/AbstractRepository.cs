using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using Birko.Data.Filters;
using Birko.Data.Models;
using Birko.Data.Stores;

namespace Birko.Data.Repositories
{
    public abstract class AbstractRepository<TViewModel, TModel> 
        : IRepository<TViewModel, TModel>
        where TModel:Models.AbstractModel, Models.ILoadable<TViewModel>
        where TViewModel:Models.ILoadable<TModel>
    {
        private bool _isReadMode = false;
        protected IDictionary<Guid?, byte[]> _modelHash = new Dictionary<Guid?, byte[]>();
        protected IStore<TModel>? Store { get; set; }

        public AbstractRepository()
        {

        }
        public bool ReadMode
        {
            get
            {
                return _isReadMode;
            }
            set
            {
                _isReadMode = value;
                if (_isReadMode && _modelHash.Any())
                {
                    _modelHash.Clear();
                }
            }
        }

        protected virtual void StoreHash(TModel data)
        {
            if (!ReadMode && data != null && data.Guid != null)
            {
                if (_modelHash == null)
                {
                    _modelHash = new Dictionary<Guid?, byte[]>();
                }
                var hash = CalulateHash(data);
                if (_modelHash.ContainsKey(data.Guid))
                {
                    _modelHash[data.Guid] = hash;
                }
                else
                {
                    _modelHash.Add(data.Guid, hash);
                }
            }
        }

        protected virtual byte[] CalulateHash(TModel data)
        {
            return Birko.Data.Helpers.StringHelper.CalculateSHA1Hash(System.Text.Json.JsonSerializer.Serialize(data));
        }

        protected virtual void RemoveHash(TModel data)
        {
            if (!ReadMode && data != null && data.Guid != null && _modelHash != null)
            {
                _modelHash.Remove(data.Guid);
            }
        }

        protected virtual bool CheckHashChange(TModel data, bool update = true)
        {
            var result = true;
            if (data != null && data.Guid != null)
            {
                var hash = CalulateHash(data);
                if (_modelHash != null && _modelHash.ContainsKey(data.Guid) && Helpers.ObjectHelper.CompareHash(_modelHash[data.Guid], hash))
                {
                    result = false;
                }
            }

            if (update)
            {
                StoreHash(data);
            }

            return result;
        }

        public virtual TViewModel CreateInstance()
        {
            return (TViewModel)Activator.CreateInstance(typeof(TViewModel), Array.Empty<object>());
        }

        public virtual TModel CreateModelInstance()
        {
            return Store.CreateInstance();
        }

        public virtual TViewModel LoadInstance(TModel model = null)
        {
            if (model != null)
            {
                return default(TViewModel);
            }
            TViewModel result = CreateInstance();
            result.LoadFrom(model);
            StoreHash(model);
            return result;
        }

        public virtual TModel LoadModelInstance(TViewModel model)
        {
            TModel result = CreateModelInstance();
            result.LoadFrom(model);
            return result;
        }

        public virtual TViewModel? ReadOne(IRepositoryFilter<TModel>? filter = null)
        {
            if (Store != null)
            {
                var model = Store?.ReadOne(filter?.Filter());
                return LoadInstance(model);
            }
            return default;
        }

        public virtual void Create(TViewModel data, ProcessDataDelegate<TModel>? processDelegate = null)
        {
            if (ReadMode)
            {
                throw new AccessViolationException("Repository is in Read Mode");
            }
            if (Store == null || data == null)
            {
                return;
            }

            TModel item = LoadModelInstance(data);
            Store.Create(item, (x) =>
            {
                x = processDelegate?.Invoke(x) ?? x;
                StoreHash(x);
                return x;
            });
            data.LoadFrom(item);
        }

        public virtual void Update(TViewModel data, ProcessDataDelegate<TModel>? processDelegate = null)
        {

            if (ReadMode)
            {
                throw new AccessViolationException("Repository is in Read Mode");
            }
            if (Store == null || data == null)
            {
                return;
            }

            TModel item = LoadModelInstance(data);
            Store.Update(item, (x) =>
            {
                x = processDelegate?.Invoke(x) ?? x;
                if (CheckHashChange(x))
                {
                    return x;
                }
                return null;
            });
            data.LoadFrom(item);
        }

        public virtual void Delete(TViewModel model)
        {
            if (ReadMode)
            {
                throw new AccessViolationException("Repository is in Read Mode");
            }
            if (Store == null)
            {
                return;
            }
            var item = (TModel)Activator.CreateInstance(model.GetType(), Array.Empty<object>());
            item.LoadFrom(model);
            Store?.Delete(item);
        }

        public virtual long Count(IRepositoryFilter<TModel>? filter = null)
        {
            return Store?.Count(filter?.Filter()) ?? 0;
        }

        public virtual void Destroy()
        {
            Store?.Destroy();
        }
    }
}
