using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Birko.Data.Repository
{
    public delegate object ProcessDataDelegate(object data);
    public interface IRepository<T>
    {
        void Read(Action<T> readAction);
        T Read(Guid Id);
        T Create(T data, ProcessDataDelegate processDelegate = null);
        T Update(Guid Id, T data, ProcessDataDelegate processDelegate = null);
        T Delete(Guid Id);
        long Count();
    }
}
