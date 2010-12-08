using System;

namespace OpenFileSystem.IO.FileSystems
{
    public interface IFileSystemNotifier
    {
        IDisposable RegisterNotification(Path path, string filter = "*", bool includeSubdirectories = false, Action<IFile> created = null, Action<IFile> modified = null, Action<IFile> deleted = null, Action<IFile> renamed=null);
    }
}