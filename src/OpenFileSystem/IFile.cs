using System;
using System.IO;

namespace OpenFileSystem.IO
{
    public interface IFile : IFileSystemItem<IFile>
    {
        string NameWithoutExtension { get; }
        DateTime? LastModifiedTimeUtc { get; }
        Stream Open(FileMode fileMode, FileAccess fileAccess, FileShare fileShare);
    }
}