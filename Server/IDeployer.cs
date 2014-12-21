using System.Collections.Generic;
using System.ServiceModel;

namespace Server
{
    [ServiceContract]
    public interface IDeployer
    {
        [OperationContract]
        void SendFile(RemoteFile remoteFile);

        [OperationContract]
        IEnumerable<FileDetail> GetAllFiles(string path);

        [OperationContract]
        void DeleteFile(string path);
    }
}