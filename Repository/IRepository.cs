using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Birko.Data.Repository
{
    public interface IRepository<T>
    {
        IEnumerable<T> Read();
        T Read(Guid Id);
        T Create(T data);
        T Update(Guid Id, T data);
        T Delete(Guid Id);
        long Count();
    }
}
