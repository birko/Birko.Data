using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Birko.Data.Model;

namespace Birko.Data.Repository
{
    public abstract class AbstractRepository<TViewModel, TModel> : IRepository<TViewModel, TModel>
        where TModel:Model.AbstractModel, Model.ILoadable<TViewModel>
        where TViewModel:Model.ILoadable<TModel>
    {
        protected string _path = null;
        protected Store.IStore<TModel> _store;
        protected IDictionary<Guid?, string> _modelHash = new Dictionary<Guid?, string>();

        public virtual void StoreHash(TModel data)
        {
            if (data != null && data.Guid != null)
            {
                if (_modelHash == null)
                {
                    _modelHash = new Dictionary<Guid?, string>();
                }
                var hash = CalculateHash(data);
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

        public virtual void RemoveHash(TModel data)
        {
            if (data != null  && data.Guid != null && _modelHash != null)
            {
                _modelHash.Remove(data.Guid);
            }
        }

        public virtual bool CheckHashChange(TModel data, bool update = true)
        {
            var result = true;
            if (data != null && data.Guid != null)
            {
                var hash = CalculateHash(data);
                if (_modelHash != null && _modelHash.ContainsKey(data.Guid) && _modelHash[data.Guid] == hash)
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


        public virtual string CalculateHash(TModel data)
        {
            var dataBytes = Encoding.ASCII.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(data));
            var sha1 = new SHA1CryptoServiceProvider();
            var sha1data = sha1.ComputeHash(dataBytes);
            return Encoding.ASCII.GetString(sha1data);
        }

        public virtual TViewModel Create(TViewModel data, ProcessDataDelegate<TModel> processDelegate = null)
        {
            if (_store != null && data != null)
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
            return data;
        }

        public TViewModel Update(Guid Id, TViewModel data, ProcessDataDelegate<TModel> processDelegate = null)
        {
            if (_store != null)
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
                    RemoveHash(item);
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
                    StoreHash(item);
                    readAction?.Invoke(result);
                });
            }
        }

        public void StoreChanges()
        {
            _store?.StoreChanges();
        }
    }
}
