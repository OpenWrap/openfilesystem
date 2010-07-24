using System.Collections.Generic;

namespace OpenFileSystem.IO
{
    public interface IDirectory : IFileSystemItem<IDirectory>
    {
        IDirectory GetDirectory(string directoryName);
        IFile GetFile(string fileName);
        IEnumerable<IFile> Files();
        IEnumerable<IDirectory> Directories();
        IEnumerable<IFile> Files(string filter);
        IEnumerable<IDirectory> Directories(string filter);
        void Add(IFile file);
        bool IsHardLink { get; }
        IDirectory LinkTo(string path);
        IDirectory Target { get; }
    }
}