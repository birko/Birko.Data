using System;
using System.Collections.Generic;
using System.Text;

namespace Birko.Data.Store
{

    public class Settings
    {
        public string Location { get; set; }
        public string Name { get; set; }

        public virtual string GetId()
        {
            return string.Format("{0}:{1}", Location, Name);
        }
    }

    public class PasswordSettings : Settings
    {
        public string Password { get; set; }
    }
}
