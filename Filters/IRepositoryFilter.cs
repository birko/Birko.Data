using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Birko.Data.Filters
{
    public interface IRepositoryFilter<TModel>
        where TModel : Models.AbstractModel
    {
        Expression<Func<TModel, bool>> Filter();
    }
}
