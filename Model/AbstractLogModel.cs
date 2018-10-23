using System;
using System.Collections.Generic;
using System.Text;
using Birko.Data.Attribute;
using Birko.Data.ViewModel;

namespace Birko.Data.Model
{
    public abstract class AbstractLogModel : AbstractModel, ICopyable<AbstractLogModel>, ILoadable<ViewModel.LogViewModel>
    {
        [DateTimeField]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [DateTimeField]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public AbstractLogModel CopyTo(AbstractLogModel clone = null)
        {
            base.CopyTo(clone);
            if (clone != null)
            {
                clone.CreatedAt = CreatedAt;
                clone.UpdatedAt = UpdatedAt;
            }
            return clone;
        }

        public void LoadFrom(LogViewModel data)
        {
            base.LoadFrom(data);
            if(data != null)
            {
                CreatedAt = data.CreatedAt;
                UpdatedAt = data.UpdatedAt;
            }
        }
    }
}
