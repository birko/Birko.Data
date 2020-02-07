using System;
using System.Collections.Generic;
using System.Text;
using Birko.Data.ViewModels;

namespace Birko.Data.Models
{
    public abstract partial class AbstractModel : ICopyable<AbstractModel>, ILoadable<ModelViewModel>
    {
        public virtual Guid? Guid { get; set; } = null;

        public AbstractModel CopyTo(AbstractModel clone = null)
        {
            if (clone != null)
            {
                clone.Guid = Guid;
            }
            return clone;
        }

        public void LoadFrom(ModelViewModel data)
        {
            if (data != null)
            {
                Guid = data.Guid;
            }
        }
    }
}
