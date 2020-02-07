using Birko.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Birko.Data.ViewModels
{
    public abstract class LogViewModel : ModelViewModel, ILoadable<AbstractLogModel>, ILoadable<LogViewModel>
    {
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime? PrevUpdatedAt { get; private set; } = null;

        public void LoadFrom(AbstractLogModel data)
        {
            base.LoadFrom(data);
            if (data != null)
            {
                CreatedAt = data.CreatedAt;
                UpdatedAt = data.UpdatedAt;
                PrevUpdatedAt = data.PrevUpdatedAt;
            }
        }

        public void LoadFrom(LogViewModel data)
        {
            base.LoadFrom(data);
            if (data != null)
            {
                CreatedAt = data.CreatedAt;
                UpdatedAt = data.UpdatedAt;
                PrevUpdatedAt = data.PrevUpdatedAt;
            }
        }
    }
}
