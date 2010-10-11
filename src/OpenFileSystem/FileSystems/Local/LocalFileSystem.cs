using System;
using System.IO;
using OpenFileSystem.IO.FileSystems.Local.Unix;
using OpenFileSystem.IO.FileSystems.Local.Win32;

namespace OpenFileSystem.IO.FileSystem.Local
{
    public abstract class LocalFileSystem : AbstractFileSystem
    {
        static LocalFileSystem _instance;
        static readonly object _syncRoot = new object();
        public static IFileSystem Instance
        {
            get
            {
                if (_instance == null)
                    lock (_syncRoot)
                        if (_instance == null)
                            _instance = CreatePlatformSpecificInstance();
                return _instance;
            }
        }

        static LocalFileSystem CreatePlatformSpecificInstance()
        {
            var platformId = (int)Environment.OSVersion.Platform;
            if (platformId == (int)PlatformID.Win32NT)
                return CreateWin32FileSystem();
            else if (platformId == 4 || platformId == 128 || platformId == (int) PlatformID.MacOSX)
                return UnixFileSystem();
            throw new NotSupportedException("Platform not supported");
        }

        static LocalFileSystem CreateWin32FileSystem()
        {
            return new Win32FileSystem();
        }

        static LocalFileSystem UnixFileSystem()
        {
            return new UnixFileSystem();
        }

        protected LocalFileSystem()
        {
            
        }
        public override IDirectory CreateDirectory(string path)
        {
            return GetDirectory(path).Create();
        }

        public override ITemporaryDirectory CreateTempDirectory()
        {
            return new TemporaryDirectory(CreateDirectory(System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName())));
        }

        public override ITemporaryFile CreateTempFile()
        {
            return new TemporaryLocalFile(System.IO.Path.GetTempFileName(), di => CreateDirectory(di.FullName));
        }

        public override IFile GetFile(string filePath)
        {
            return new LocalFile(System.IO.Path.GetFullPath(System.IO.Path.Combine(Environment.CurrentDirectory, filePath)), di=>CreateDirectory(di.FullName));
        }

        public override Path GetPath(string path)
        {
            return new Path(path);
        }
    }
}