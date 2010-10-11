using OpenFileSystem.IO.FileSystem.Local;

namespace OpenFileSystem.IO.FileSystem
{
    public abstract class AbstractFileSystem : IFileSystem
    {
        public abstract ITemporaryDirectory CreateTempDirectory();
        public abstract IDirectory CreateDirectory(string path);
        public abstract ITemporaryFile CreateTempFile();
        public abstract IDirectory GetDirectory(string directoryPath);
        public abstract IFile GetFile(string itemSpec);
        public abstract Path GetPath(string path);
        public abstract IDirectory GetTempDirectory();
    }
}