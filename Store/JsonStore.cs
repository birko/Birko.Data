using CashRegister.Core.ViewModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Birko.Data.Store
{
    public class JsonStore<T> : IStore<T>
        where T: Model.AbstractModel
    {
        private readonly Settings _settings;
        private List<T> _items = new List<T>();

        public string Path
        {
            get
            {
                return (!string.IsNullOrEmpty(_settings?.Location) && !string.IsNullOrEmpty(_settings?.Name))
                    ? System.IO.Path.Combine(_settings.Location, _settings.Name)
                    : null;
            }
        }

        public JsonStore(Settings settings)
        {
            _settings = settings;
            Init();
            Load();
        }

        public void Init()
        {
            if (!string.IsNullOrEmpty(Path) && !System.IO.File.Exists(Path))
            {
                System.IO.File.WriteAllText(Path, "[]");
            }
        }

        public IEnumerable<T> List()
        {
            return _items.AsEnumerable();
        }

        public long Count()
        {
            return _items?.Count ?? 0;
        }


        public long Count(Expression<Func<T, bool>> filter)
        {
            return _items?.Where(filter.Compile()).Count() ?? 0;
        }

        public void Save(T data)
        {
            if (data.Guid == null || !_items.Any(x=>x.Guid == data.Guid)) // new
            {
                data.Guid = Guid.NewGuid();
                _items.Add(data);
            }
            else //update
            {
                var item = _items.FirstOrDefault(x => x.Guid == data.Guid);
                if (item is Model.AbstractLogModel)
                {
                    (item as Model.AbstractLogModel).UpdatedAt = DateTime.UtcNow;
                }
                data.CopyTo(item);
            }
        }

        public void Delete(T data)
        {
            if (data.Guid != null && _items != null && _items.Any(x => x.Guid == data.Guid))
            {
                var item = _items.FirstOrDefault(x => x.Guid == data.Guid);
                _items.Remove(item);
            }
        }

        public void Load()
        {
            if (!string.IsNullOrEmpty(Path) && System.IO.File.Exists(Path))
            {
                using (System.IO.StreamReader file = System.IO.File.OpenText(Path))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    _items = (List<T>)serializer.Deserialize(file, typeof(List<T>));
                }
            }
            if (_items == null)
            {
                _items = new List<T>();
            }
        }

        public void StoreChanges()
        {
            if (!string.IsNullOrEmpty(Path) && System.IO.File.Exists(Path))
            {
                using (System.IO.TextWriter file = System.IO.File.CreateText(Path))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(file, _items);
                }
            }
        }
    }
}
