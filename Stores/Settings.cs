using System;
using System.Collections.Generic;
using System.Text;

namespace Birko.Data.Stores
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

    public class RemoteSettings : PasswordSettings
    {
        public string UserName { get; set; }
        public int Port { get; set; }

        public override string GetId()
        {
            return string.Format("{0}:{1}:{2}", base.GetId(), UserName, Port);
        }
    }
}
