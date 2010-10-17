using System;
using OpenFileSystem.IO.FileSystem.Local;

namespace OpenFileSystem.IO
{
    public interface ITemporaryFile : IFile, IDisposable
    {
    }
}