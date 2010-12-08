using System;

namespace OpenFileSystem.IO.FileSystems
{
    public interface IFileSytemNotifierEntry
    {
        IDisposable AddNotifiers(params Action<IFile>[] entries);
    }
}