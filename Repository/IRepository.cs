using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Birko.Data.Repository
{
    public delegate TModel ProcessDataDelegate<TModel>(TModel data) where TModel: Model.AbstractModel;
    public interface IRepository<T, TModel>
        where TModel : Model.AbstractModel
    {
        void Read(Action<T> readAction);
        T Read(Guid Id);
        T Create(T data, ProcessDataDelegate<TModel> processDelegate = null);
        T Update(Guid Id, T data, ProcessDataDelegate<TModel> processDelegate = null);
        T Delete(Guid Id);
        long Count();
    }
}
