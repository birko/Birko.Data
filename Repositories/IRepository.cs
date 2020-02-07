using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Birko.Data.Repositories
{
    public delegate TModel ProcessDataDelegate<TModel>(TModel data) where TModel: Models.AbstractModel;

    public interface IRepository<T, TModel>
        where TModel : Models.AbstractModel
    {
        void Read(Action<T> readAction);
        void Read(Expression<Func<TModel, bool>> expr,  Action<T> readAction);
        T Read(Guid Id);
        T Create(T data, ProcessDataDelegate<TModel> processDelegate = null);
        T Update(Guid Id, T data, ProcessDataDelegate<TModel> processDelegate = null);
        T Delete(Guid Id);
        long Count();

        void Destroy();
    }
}
