using Birko.Data.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Birko.Data.ViewModel
{
    public abstract class LogViewModel : ModelViewModel, ILoadable<AbstractLogModel>
    {
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

        public void LoadFrom(AbstractLogModel data)
        {
            base.LoadFrom(data);
            if (data != null)
            {
                CreatedAt = data.CreatedAt;
                UpdatedAt = data.UpdatedAt;
            }
        }
    }
}
