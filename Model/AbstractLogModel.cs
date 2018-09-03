using System;
using System.Collections.Generic;
using System.Text;
using Birko.Data.Attribute;

namespace Birko.Data.Model
{
    public abstract class AbstractLogModel : AbstractModel, ICopyable<AbstractLogModel>
    {
        [DateTimeField]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [DateTimeField]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public virtual AbstractLogModel CopyTo(AbstractLogModel clone = null)
        {
            base.CopyTo(clone);
            if (clone != null)
            {
                clone.CreatedAt = CreatedAt;
                clone.UpdatedAt = UpdatedAt;
            }
            return clone;
        }
    }
}
