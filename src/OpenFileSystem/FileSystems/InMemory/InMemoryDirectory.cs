using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenFileSystem.IO.FileSystems.InMemory
{
    public class InMemoryDirectory : AbstractDirectory, IDirectory, IEquatable<IDirectory>
    {
        InMemoryDirectory _source;

        public InMemoryDirectory(InMemoryFileSystem fileSystem, string directoryPath)
        {
            _source = this;
            directoryPath = NormalizeDirectoryPath(directoryPath);
            Path = new Path(directoryPath);

            ChildDirectories = new List<InMemoryDirectory>();
            ChildFiles = new List<InMemoryFile>();
            _fileSystem = fileSystem;
            //foreach (var childDirectory in children.OfType<InMemoryDirectory>())
            //{
            //    childDirectory.Parent = this;
            //    childDirectory.Create();
            //    ChildDirectories.Add(childDirectory);
            //}
            //foreach (var childFile in children.OfType<InMemoryFile>())
            //{
            //    childFile.Parent = this;
            //    childFile.Path = Path.Combine(childFile.Path.FullPath);
            //    childFile.Create();
            //    ChildFiles.Add(childFile);
            //}
        }

        public List<InMemoryDirectory> ChildDirectories { get; set; }
        public List<InMemoryFile> ChildFiles { get; set; }

        public bool Exists { get; set; }
        InMemoryFileSystem _fileSystem;
        public IFileSystem FileSystem { get { return _fileSystem; } }
        public bool IsHardLink { get; private set; }

        public string Name
        {
            get { return new DirectoryInfo(Path.FullPath).Name; }
        }

        public IDirectory Parent { get; set; }

        public Path Path { get; private set; }
        IDirectory _target = null;
        StringComparison _stringComparison = StringComparison.OrdinalIgnoreCase;

        public IDirectory Target
        {
            get { return _target ?? this; }
            set { _target = value; }
        }

        public IDisposable FileChanges(string filter = "*", bool includeSubdirectories = false, Action<IFile> created = null, Action<IFile> modified = null, Action<IFile> deleted = null, Action<IFile> renamed = null)
        {
            return _fileSystem.Notifier.RegisterNotification(Path, filter, includeSubdirectories, created, modified, deleted, renamed);
        }


        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (!(obj is IDirectory)) return false;
            return Equals((IDirectory)obj);
        }

        public override int GetHashCode()
        {
            return Path.GetHashCode();
        }

        public IEnumerable<IDirectory> Directories()
        {
            return ChildDirectories.Where(x => x.Exists).Cast<IDirectory>();
        }

        public IEnumerable<IDirectory> Directories(string filter, SearchScope scope)
        {
            var filterRegex = filter.Wildcard();
            var immediateChildren = ChildDirectories.Where(x => x.Exists && filterRegex.IsMatch(x.Name)).Cast<IDirectory>();
            return scope == SearchScope.CurrentOnly
                       ? immediateChildren
                       : immediateChildren
                             .Concat(ChildDirectories.SelectMany(x => x.Directories(filter, scope)));
        }

        public IEnumerable<IFile> Files()
        {
            return ChildFiles.Where(x => x.Exists).Cast<IFile>();
        }

        public IEnumerable<IFile> Files(string filter, SearchScope searchScope)
        {
            var filterRegex = filter.Wildcard();
            var immediateChildren = ChildFiles.Where(x => x.Exists && filterRegex.IsMatch(x.Name)).Cast<IFile>();
            return searchScope == SearchScope.CurrentOnly
                ? immediateChildren
                : immediateChildren.Concat(ChildDirectories.SelectMany(x => x.Files(filter, searchScope)));
        }

        public IDirectory GetDirectory(string directoryName)
        {
            if (System.IO.Path.IsPathRooted(directoryName))
                return FileSystem.GetDirectory(directoryName);

            var inMemoryDirectory =
                    ChildDirectories.FirstOrDefault(x => x.Name.Equals(directoryName, _stringComparison));


            if (inMemoryDirectory == null)
            {
                inMemoryDirectory = new InMemoryDirectory(_fileSystem, System.IO.Path.Combine(Path.FullPath, directoryName))
                {
                        Parent = this
                };
                ChildDirectories.Add(inMemoryDirectory);
            }


            return inMemoryDirectory;
        }
        public IFile GetFile(string fileName)
        {
            var file = ChildFiles.FirstOrDefault(x => x.Name.Equals(fileName, _stringComparison));
            if (file == null)
            {
                file = new InMemoryFile(Path.Combine(fileName).FullPath) { Parent = this };
                ChildFiles.Add(file);
            }
            file.FileSystem = this.FileSystem;

            return file;
        }

        public IDirectory LinkTo(string path)
        {
            var linkDirectory = (InMemoryDirectory)GetDirectory(path);
            if (linkDirectory.Exists)
                throw new IOException(string.Format("Cannot create link at location '{0}', a directory already exists.", path));
            linkDirectory.ChildDirectories = this.ChildDirectories;
            linkDirectory.ChildFiles = this.ChildFiles;
            linkDirectory.IsHardLink = true;
            linkDirectory.Exists = true;
            linkDirectory.Target = this;
            return linkDirectory;
        }

        public bool Equals(IDirectory other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Path, Path);
        }

        public void Delete()
        {
            if (!IsHardLink)
            {
                foreach (var childDirectory in ChildDirectories.Copy())
                    childDirectory.Delete();
            }
            Exists = false;
        }

        public void CopyTo(IFileSystemItem item)
        {
            throw new NotImplementedException();
        }

        public void MoveTo(IFileSystemItem item)
        {
            if (!item.Path.IsRooted) throw new ArgumentException("Has to be a fully-qualified path for a move to succeed.");
            var newDirectory = (InMemoryDirectory)FileSystem.GetDirectory(item.Path.FullPath);
            newDirectory.Exists = true;
            newDirectory.ChildFiles = this.ChildFiles;
            newDirectory.ChildDirectories = this.ChildDirectories;
            foreach(var file in newDirectory.ChildFiles.OfType<InMemoryFile>())
                file.Parent = newDirectory;
            
            foreach(var file in newDirectory.ChildDirectories.OfType<InMemoryDirectory>())
                file.Parent = newDirectory;

            this.Exists = false;
            ChildFiles.Clear();
            ChildDirectories.Clear();
        }

        public IDirectory Create()
        {
            Exists = true;
            if (Parent != null && !Parent.Exists)
                Parent.Create();
            return this;
        }

        public void Move(Path newFileName)
        {
        }

        public override string ToString()
        {
            return Path.FullPath;
        }
    }
}