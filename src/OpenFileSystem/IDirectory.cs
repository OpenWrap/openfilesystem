using System.Collections.Generic;
using OpenFileSystem.IO.FileSystems;

namespace OpenFileSystem.IO
{
    public interface IDirectory : IFileSystemItem<IDirectory>
    {
        IDirectory GetDirectory(string directoryName);
        IFile GetFile(string fileName);
        IEnumerable<IFile> Files();
        IEnumerable<IDirectory> Directories();
        IEnumerable<IFile> Files(string filter, SearchScope scope);
        IEnumerable<IDirectory> Directories(string filter, SearchScope scope);
        void Add(IFile file);
        bool IsHardLink { get; }
        IDirectory LinkTo(string path);
        IDirectory Target { get; }
    }
    public static class DirectoryExtensions
    {
        public static IEnumerable<IFile> Files(this IDirectory directory, string filter)
        {
            return directory.Files(filter, SearchScope.CurrentOnly);
        }
        public static IEnumerable<IDirectory> Directories(this IDirectory directory, string filter)
        {
            return directory.Directories(filter, SearchScope.CurrentOnly);
        }
    }
}