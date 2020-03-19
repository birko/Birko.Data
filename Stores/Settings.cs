using System;
using System.Collections.Generic;
using System.Text;

namespace Birko.Data.Stores
{

    public interface ISettings : Models.ILoadable<ISettings>
    {
        string GetId();

    }

    public class Settings : ISettings, Models.ILoadable<Settings>
    {
        public string Location { get; set; }
        public string Name { get; set; }

        public Settings()
        {
        }

        public Settings(string location, string name) : this()
        {
            Location = location;
            Name = name;
        }
        public virtual string GetId()
        {
            return string.Format("{0}:{1}", Location, Name);
        }

        public void LoadFrom(Settings data)
        {
            if (data != null)
            {
                Location = data.Location;
                Name = data.Name;
            }
        }

        public void LoadFrom(ISettings data)
        {
            LoadFrom((Settings)data);
        }
    }

    public class PasswordSettings : Settings, Models.ILoadable<PasswordSettings>
    {
        public string Password { get; set; }

        public PasswordSettings(string location, string name, string password = null) : base(location, name)
        {
            Password = password;
        }

        public void LoadFrom(PasswordSettings data)
        {
            base.LoadFrom(data);
            if (data != null)
            {
                Password = data.Password;
            }
        }
    }

    public class RemoteSettings : PasswordSettings, Models.ILoadable<RemoteSettings>
    {
        public string UserName { get; set; }
        public int Port { get; set; }

        public RemoteSettings(string location, string name, string username, string password, int port) : base(location, name, password)
        {
            UserName = username;
            Port = port;
        }
        public override string GetId()
        {
            return string.Format("{0}:{1}:{2}", base.GetId(), UserName, Port);
        }

        public void LoadFrom(RemoteSettings data)
        {
            base.LoadFrom(data);
            if (data != null)
            {
                UserName = data.UserName;
                Port = data.Port;
            }
        }
    }
}
