using Birko.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Birko.Data.Filters
{
    public class ModelsByGuid<TModel> : IRepositoryFilter<TModel>
         where TModel : AbstractModel
    {
        public IEnumerable<Guid> Guids { get; set; }

        public ModelsByGuid(IEnumerable<Guid> guids)
        {
            Guids = guids;
        }

        public virtual Expression<Func<TModel, bool>> Filter()
        {
            return (x) => Guids != null && x.Guid != null && Guids.Contains(x.Guid.Value);
        }
    }
}
