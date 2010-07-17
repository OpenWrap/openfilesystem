using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenFileSystem.IO.FileSystem.Local;

namespace OpenFileSystem.IO.FileSystems.Local.Unix
{
    public class UnixFileSystem : LocalFileSystem
    {
        public override IDirectory GetDirectory(string directoryPath)
        {
            throw new NotImplementedException();
        }

        public override IDirectory GetTempDirectory()
        {
            throw new NotImplementedException();
        }
    }
}
