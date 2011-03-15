using System;
using System.IO;

namespace OpenFileSystem.IO
{
    public interface IFile : IFileSystemItem<IFile>
    {
        string NameWithoutExtension { get; }
        string Extension { get; }
        long Size { get; }
        DateTimeOffset? LastModifiedTimeUtc { get; set; }
        Stream Open(FileMode fileMode, FileAccess fileAccess, FileShare fileShare);
    }
}