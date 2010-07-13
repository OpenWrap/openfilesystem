using System.Collections.Generic;

namespace OpenFileSystem.IO
{
    public interface IPath
    {
        string FullPath { get; }
        IPath Combine(params string[] paths);
        IFileSystem FileSystem { get; }
        IEnumerable<string> Segments { get; }
        bool IsRooted { get; }
    }
}