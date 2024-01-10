using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Birko.Data.Stores
{
    public interface IBulkReadStore<T>
         where T : Models.AbstractModel
    {
        IEnumerable<T> Read(Expression<Func<T, bool>>? filter = null, int? limit = null, int? offset = null);
    }

    public interface IBulkDeleteStore<T>
         where T : Models.AbstractModel
    {
        void Delete(IEnumerable<T> data);
    }

    public interface IBulkCreateStore<T>
         where T : Models.AbstractModel
    {
        void Create(IEnumerable<T> data, StoreDataDelegate<T>? storeDelegate = null);
    }

    public interface IBulkUpdateStore<T>
         where T : Models.AbstractModel
    {
        void Update(IEnumerable<T> data, StoreDataDelegate<T>? storeDelegate = null);
    }

    public interface IBulkStore<T>
        : IStore<T>
        , IBulkReadStore<T>
        , IBulkCreateStore<T>
        , IBulkUpdateStore<T>
        , IBulkDeleteStore<T>
        where T : Models.AbstractModel
    {
    }
}
