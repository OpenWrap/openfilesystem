using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Mono.Unix;
using OpenFileSystem.IO.FileSystem.Local;

namespace OpenFileSystem.IO.FileSystems.Local.Unix
{
    public class UnixFileSystem : LocalFileSystem
    {
        public override IDirectory GetDirectory(string directoryPath)
        {
            return new UnixDirectory(directoryPath);
        }

        public override IDirectory GetTempDirectory()
        {
            throw new NotImplementedException();
        }
    }

    public class UnixDirectory : LocalDirectory
    {
        public UnixDirectory(DirectoryInfo di) : base(di)
        {
            GetUnixInfo(di.FullName);

        }

        void GetUnixInfo(string fullName)
        {
            this.UnixDirectoryInfo = UnixFileSystemInfo.GetFileSystemEntry(fullName);

        }

        protected UnixFileSystemInfo UnixDirectoryInfo { get; set; }

        public UnixDirectory(string directoryPath) : base(directoryPath)
        {
            
        }
        protected override LocalDirectory CreateDirectory(string path)
        {
            return new UnixDirectory(path);
        }
        protected override LocalDirectory CreateDirectory(System.IO.DirectoryInfo di)
        {
            return new UnixDirectory(di);
        }
        public override bool IsHardLink
        {
            get { return UnixDirectoryInfo.IsSymbolicLink; }
        }
        public override IDirectory LinkTo(string path)
        {
            UnixDirectoryInfo.CreateSymbolicLink(path);
            return CreateDirectory(path);
        }
    }
}
