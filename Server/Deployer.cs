using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using Logic;

namespace Server
{
    [MessageContract]
    public class RemoteFile : IDisposable
    {
        [MessageHeader(MustUnderstand = true)] public string FullPath;

        [MessageBodyMember(Order = 1)] public Stream Stream;

        public void Dispose()
        {
            if (Stream != null)
            {
                Stream.Close();
                Stream = null;
            }
        }
    }

    public class Deployer : IDeployer
    {
        public IEnumerable<FileDetail> GetAllFiles(string path)
        {
            return Directory.GetFiles(path, "*", SearchOption.AllDirectories).Select(f => new FileDetail
            {
                Path = f,
                Hash = f.MD5Hash(),
            });
        }

        public void SendFile(RemoteFile remoteFile)
        {
            var path = new FileInfo(remoteFile.FullPath);
            if (!Directory.Exists(path.Directory.FullName))
                Directory.CreateDirectory(path.Directory.FullName);
            using (FileStream fileStream = File.Create(remoteFile.FullPath))
            {
                remoteFile.Stream.CopyTo(fileStream);
            }
        }
    }
}