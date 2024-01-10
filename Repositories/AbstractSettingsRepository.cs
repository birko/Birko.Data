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
    public abstract class AbstractStoreRepository<TViewModel, TModel>
        : AbstractRepository<TViewModel, TModel>
        , ISettingsRepository<TViewModel, TModel, Stores.ISettings>
        where TModel:Models.AbstractModel, Models.ILoadable<TViewModel>
        where TViewModel:Models.ILoadable<TModel>
    {
        protected Settings Settings { get; set; }

        public AbstractStoreRepository() : base()
        {

        }

        public virtual void SetSettings(Stores.ISettings settings)
        {
            if (settings is Settings setts)
            {
                Settings = setts;
            }
        }
    }
}
