using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using OpenFileSystem.IO.FileSystem.Local;
using Path = OpenFileSystem.IO.FileSystem.Local.Path;

namespace OpenFileSystem.IO.FileSystem.InMemory
{
    public class InMemoryFileSystem : IFileSystem
    {
        readonly object _syncRoot = new object();
        public Dictionary<string, InMemoryDirectory> Directories { get; private set; }

        public InMemoryFileSystem(params InMemoryDirectory[] childDirectories)
        {
            Directories = new Dictionary<string, InMemoryDirectory>(StringComparer.OrdinalIgnoreCase);
            CurrentDirectory = @"c:\";
            
            foreach(var directory in childDirectories)
            {
                var root = GetRoot(directory.Path.Segments.First());
                foreach(var segment in directory.Path.Segments
                    .Skip(1)
                    .Take(directory.Path.Segments.Count()-2))
                {
                    root = (InMemoryDirectory)root.GetDirectory(segment);
                }
                directory.Parent = root;
                root.ChildDirectories.Add(directory);
                directory.Create();
            }
            Action<IEnumerable<IDirectory>> assignFs = null;
            assignFs = dirs =>
            {
                foreach (var dir in dirs.OfType<InMemoryDirectory>())
                {
                    dir.FileSystem = this;
                    assignFs(dir.Directories());
                }
            };
            assignFs(childDirectories);
        }
        
        InMemoryDirectory GetRoot(string path)
        {
            InMemoryDirectory directory;
            if (!Directories.TryGetValue(path, out directory))
            {
                Directories.Add(path, directory = new InMemoryDirectory(path)
                {
                    FileSystem = this
                });
            }
            return directory;
        }
        public IDirectory GetDirectory(string directoryPath)
        {
            var resolvedDirectoryPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(CurrentDirectory,directoryPath));
            var pathSegments = new Path(resolvedDirectoryPath).Segments;
            return pathSegments
                .Skip(1)
                .Aggregate((IDirectory)GetRoot(pathSegments.First()),
                    (current, segment) => current.GetDirectory(segment));
        }

        public IFile GetFile(string filePath)
        {
            var resolvedFilePath = System.IO.Path.GetFullPath(System.IO.Path.Combine(CurrentDirectory, filePath));
            var pathSegments = new Path(resolvedFilePath).Segments;
            var ownerFolder = pathSegments
                .Skip(1).Take(pathSegments.Count()-2)
                .Aggregate((IDirectory)GetRoot(pathSegments.First()),
                    (current, segment) => current.GetDirectory(segment));
            return ownerFolder.GetFile(pathSegments.Last());
        }

        bool DirectoryExists(string resolvedDirectoryPath)
        {
            throw new InvalidOperationException();

        }

        public Path GetPath(string path)
        {
            return new Path(path);
        }

        public ITemporaryDirectory CreateTempDirectory()
        {
            var sysTemp = (InMemoryDirectory)GetTempDirectory();

            var tempDirectory = new InMemoryTemporaryDirectory(sysTemp.Path.Combine(System.IO.Path.GetRandomFileName()).FullPath)
            {
                Exists = true,
                FileSystem = this,
                Parent = sysTemp
            };
            sysTemp.ChildDirectories.Add(tempDirectory);
            return tempDirectory;
        }

        public IDirectory CreateDirectory(string path)
        {
            return GetDirectory(path).MustExist();
        }

        public ITemporaryFile CreateTempFile()
        {
            var tempDirectory = (InMemoryDirectory)GetTempDirectory();
            var tempFile = new InMemoryTemporaryFile(tempDirectory.Path.Combine(System.IO.Path.GetRandomFileName()).ToString())
            {
                Exists = true,
                FileSystem = this,
                Parent = tempDirectory
            };
            tempDirectory.Create();
            tempDirectory.ChildFiles.Add(tempFile);
            
            return tempFile;
        }

        IDirectory _systemTempDirectory;
        public IDirectory GetTempDirectory()
        {
            if(_systemTempDirectory == null)
            {
                lock(_syncRoot)
                {
                    Thread.MemoryBarrier();
                    if (_systemTempDirectory == null)
                        _systemTempDirectory = GetDirectory(System.IO.Path.GetTempPath());
                }
            }
            return _systemTempDirectory;
        }

        public string CurrentDirectory { get; set; }
    }
}