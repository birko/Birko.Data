using Birko.Data.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Birko.Data.ViewModel
{
    public abstract class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
