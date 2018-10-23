using System;
using System.Collections.Generic;
using System.Text;
using Birko.Data.Attribute;
using Birko.Data.ViewModel;

namespace Birko.Data.Model
{
    public abstract class AbstractModel : ICopyable<AbstractModel>, ILoadable<ModelViewModel>
    {
        [GuidField(null, true, true)]
        public Guid? Guid { get; set; } = null;

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
