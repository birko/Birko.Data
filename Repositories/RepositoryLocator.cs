using System;
using System.Collections.Generic;
using System.Text;

namespace Birko.Data.Repositories
{
    public static class RepositoryLocator
    {
        private static IDictionary<string, IDictionary<Type, object>> _repositories;

        public static TRepository GetRepository<TRepository>()
            where TRepository : IBaseRepository
        {

            return GetRepository<TRepository, Stores.ISettings>(null);
        }


        public static TRepository GetRepository<TRepository, TSettings>(TSettings settings)
            where TSettings : Stores.ISettings
            where TRepository : IBaseRepository

        {
            if (_repositories == null) {
                _repositories = new Dictionary<string, IDictionary<Type, object>>();
            }
            var id = settings?.GetId() ?? string.Empty;
            if (!_repositories.ContainsKey(id))
            {
                _repositories.Add(id, new Dictionary<Type, object>());
            }
            var type = typeof(TRepository);
            if (!_repositories[id].ContainsKey(type)) {
                _repositories[id].Add(type, (TRepository)Activator.CreateInstance(type, new object[] { }));
                if (!string.IsNullOrEmpty(id))
                {
                    ((IStoreRepository<TSettings>) _repositories[id][type]).SetSettings(settings);
                }

            }
            return (TRepository)_repositories[id][type];
        }
    }
}
