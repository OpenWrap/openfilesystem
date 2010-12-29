using System;

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
}