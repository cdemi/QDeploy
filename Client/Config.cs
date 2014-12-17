
using System.Collections.Generic;
using System.ComponentModel;

namespace Client
{
    public class RemoteDeployment
    {
        public string FriendlyName { get; set; }
        public string Host { get; set; }
        public string Path { get; set; }
    }
    public class Config
    {
        public string LocalDeployment { get; set; }
        public BindingList<RemoteDeployment> RemoteDeployments { get; set; }

        public Config()
        {
            RemoteDeployments = new BindingList<RemoteDeployment>();
        }
    }
}
