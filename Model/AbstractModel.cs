using System;
using System.Collections.Generic;
using System.Text;
using Birko.Data.Attribute;

namespace Birko.Data.Model
{
    public abstract class AbstractModel : ICopyable<AbstractModel>
    {
        [GuidField(null, true, true)]
        public Guid? Guid { get; set; } = null;

        public virtual AbstractModel CopyTo(AbstractModel clone = null)
        {
            if (clone != null)
            {
                clone.Guid = Guid;
            }
            return clone;
        }
    }
}
