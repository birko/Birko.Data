using System;
using System.Collections.Generic;
using System.Text;

namespace Birko.Data.Stores
{
    public static class StoreLocator
    {
        private static IDictionary<string, IDictionary<Type, object>> _stores;


        public static TStore GetStore<TStore>()
            where TStore : IBaseStore

        {
            return GetStore<TStore, ISettings>(null);
        }

        public static TStore GetStore<TStore, TSettings>(TSettings settings)
            where TSettings : ISettings
            where TStore: IBaseStore

        {
            if (_stores == null)
            {
                _stores = new Dictionary<string, IDictionary<Type, object>>();
            }
            var id = settings?.GetId() ?? string.Empty;
            if (!_stores.ContainsKey(id))
            {
                _stores.Add(id, new Dictionary<Type, object>());
            }
            var type = typeof(TStore);
            if (!_stores[id].ContainsKey(type))
            {
                _stores[id].Add(type, (TStore)Activator.CreateInstance(type, new object[] { }));
                if (!string.IsNullOrEmpty(id))
                {
                    ((ISettingsStore<ISettings>)_stores[id][type]).SetSettings(settings);
                }
            }
            return (TStore)_stores[id][type];
        }
    }
}
