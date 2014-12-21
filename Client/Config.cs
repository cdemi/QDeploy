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
        public Config()
        {
            RemoteDeployments = new BindingList<RemoteDeployment>();
            ExclusionList = new List<string>();
        }

        public string LocalDeployment { get; set; }
        public BindingList<RemoteDeployment> RemoteDeployments { get; set; }
        public List<string> ExclusionList { get; set; }

        public string AppOfflineURL { get; set; }

        public bool PutOfflineBeforeDeployment { get; set; }

        public bool PutOnlineAfterDeployment { get; set; }

        public bool SmartOffline { get; set; }
    }
}