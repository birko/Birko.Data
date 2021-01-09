using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using Birko.Data.Models;
using Birko.Data.Stores;

namespace Birko.Data.Repositories
{
    public abstract class AbstractRepository<TViewModel, TModel> : IRepository<TViewModel, TModel, Stores.ISettings>
        where TModel:Models.AbstractModel, Models.ILoadable<TViewModel>
        where TViewModel:Models.ILoadable<TModel>
    {
        private bool _isReadMode = false;
        protected string _path = null;
        protected IDictionary<Guid?, byte[]> _modelHash = new Dictionary<Guid?, byte[]>();
        protected IStore<TModel, Stores.ISettings> Store { get; set; }

        public AbstractRepository()
        {

        }


        public virtual void SetSettings(Stores.ISettings settings)
        {
            if (settings is Settings setts)
            {
                _path = setts.Location;
            }
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

        public virtual void StoreHash(TModel data)
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

        public virtual byte[] CalulateHash(TModel data)
        {
            return Birko.Data.Helpers.StringHelper.CalculateSHA1Hash(System.Text.Json.JsonSerializer.Serialize(data));
        }

        public virtual void RemoveHash(TModel data)
        {
            if (!ReadMode && data != null && data.Guid != null && _modelHash != null)
            {
                _modelHash.Remove(data.Guid);
            }
        }

        public virtual bool CheckHashChange(TModel data, bool update = true)
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


        public virtual TViewModel Create(TViewModel data, ProcessDataDelegate<TModel> processDelegate = null)
        {
            var _store = Store;
            if (!ReadMode && _store != null && data != null)
            {
                TModel item = (TModel)Activator.CreateInstance(typeof(TModel), new object[] { });
                item.LoadFrom(data);
                _store.Save(item, (x) => {
                    x = processDelegate?.Invoke(x) ?? x;
                    StoreHash(x);
                    return x;
                });
                StoreChanges();
                data.LoadFrom(item);
            }
            else if (ReadMode)
            {
                throw new AccessViolationException("Repository is in Read Mode");
            }
            return data;
        }

        public TViewModel Update(Guid Id, TViewModel data, ProcessDataDelegate<TModel> processDelegate = null)
        {
            var _store = Store;
            if (!ReadMode && _store != null)
            {
                TModel item = (TModel)Activator.CreateInstance(typeof(TModel), new object[] { });
                item.LoadFrom(data);
                _store.Save(item, (x) => {
                    x = processDelegate?.Invoke(x) ?? x;
                    if (CheckHashChange(x))
                    {
                        return x;
                    }
                    return null;
                });
                StoreChanges();
                data.LoadFrom(item);
            }
            else if (ReadMode)
            {
                throw new AccessViolationException("Repository is in Read Mode");
            }
            return data;
        }

        public virtual TViewModel Delete(Guid Id)
        {
            var _store = Store;
            if (!ReadMode && _store != null)
            {
                TViewModel result = default;
                _store.List(x => x.Guid == Id, (item) =>
                {
                    result = (TViewModel)Activator.CreateInstance(typeof(TViewModel), new object[] { });
                    _store.Delete(item);
                    result.LoadFrom(item);
                    RemoveHash(item);
                });
                StoreChanges();
                return result;
            }
            else if (ReadMode)
            {
                throw new AccessViolationException("Repository is in Read Mode");
            }
            return default;
        }

        public virtual long Count()
        {
            return Count(null);
        }

        public virtual long Count(Expression<Func<TModel, bool>> filter)
        {
            var _store = Store;
            return (_store != null) ? _store.Count(filter) : 0;
        }

        public virtual TViewModel Read(Guid Id)
        {
            var _store = Store;
            if (_store != null && _store.Count(x => x.Guid == Id) > 0)
            {
                TViewModel result = (TViewModel)Activator.CreateInstance(typeof(TViewModel), new object[] { });
                Read(x => x.Guid == Id, (item) =>
                {
                    result = item;
                }, 1, 0);

                return result;
            }
            return default;
        }

        public virtual void Read(Action<TViewModel> readAction, int? limit = null, int? offset = null)
        {
            Read(null, readAction, limit, offset);
        }

        public virtual void Read(Expression<Func<TModel, bool>> expr, Action<TViewModel> readAction, int? limit = null, int? offset = null)
        {
            var _store =  Store;
            if (_store != null && _store.Count(expr) > 0 && readAction != null)
            {
                _store.List(expr, (item) =>
                {
                    TViewModel result = (TViewModel)Activator.CreateInstance(typeof(TViewModel), new object[] { });
                    result.LoadFrom(item);
                    StoreHash(item);
                    readAction?.Invoke(result);
                }, limit, offset);
            }
        }

        public void StoreChanges()
        {
            var _store = Store;
            _store?.StoreChanges();
        }

        public virtual void Destroy()
        {
            var _store = Store;
            _store?.Destroy();
        }
    }
}
