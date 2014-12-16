using System.Collections.Generic;
using System.IO;
using System.ServiceModel;

namespace Logic
{
    [ServiceContract]
    public interface IDeployer
    {
        [OperationContract]
        void SendFile(RemoteFile remoteFile);

        [OperationContract]
        IEnumerable<FileDetail> GetAllFiles(string path);
    }
}