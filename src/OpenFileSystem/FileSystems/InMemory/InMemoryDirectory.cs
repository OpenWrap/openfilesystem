using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenFileSystem.IO.FileSystem.Local;
using OpenFileSystem.IO.FileSystems;

namespace OpenFileSystem.IO.FileSystem.InMemory
{
    public class InMemoryDirectory : AbstractDirectory, IDirectory, IEquatable<IDirectory>
    {
        InMemoryDirectory _source;

        public InMemoryDirectory(string directoryPath, params IFileSystemItem[] children)
        {
            _source = this;
            directoryPath = NormalizeDirectoryPath(directoryPath);
            Path = new LocalPath(directoryPath);

            ChildDirectories = new List<InMemoryDirectory>();
            ChildFiles = new List<InMemoryFile>();

            foreach (var childDirectory in children.OfType<InMemoryDirectory>())
            {
                childDirectory.Parent = this;
                childDirectory.Create();
                ChildDirectories.Add(childDirectory);
            }
            foreach (var childFile in children.OfType<InMemoryFile>())
            {
                childFile.Parent = this;
                childFile.Path = Path.Combine(childFile.Path.FullPath);
                childFile.Create();
                ChildFiles.Add(childFile);
            }
        }

        public List<InMemoryDirectory> ChildDirectories { get; set; }
        public List<InMemoryFile> ChildFiles { get; set; }

        public bool Exists { get; set; }
        public IFileSystem FileSystem { get; set; }
        public bool IsHardLink { get; private set; }

        public string Name
        {
            get { return new DirectoryInfo(Path.FullPath).Name; }
        }

        public IDirectory Parent { get; set; }
        public IPath Path { get; private set; }

        IDirectory _target = null;
        StringComparison _stringComparison = StringComparison.OrdinalIgnoreCase;

        public IDirectory Target
        {
            get { return _target ?? this; }
            set { _target = value; }
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

        public void Add(IFile file)
        {
            ChildFiles.Add((InMemoryFile)file);
        }

        public IEnumerable<IDirectory> Directories()
        {
            return ChildDirectories.Where(x => x.Exists).Cast<IDirectory>();
        }

        public IEnumerable<IDirectory> Directories(string filter, SearchScope scope)
        {
            if (scope == SearchScope.CurrentOnly)
            {
                var filterRegex = filter.Wildcard();
                return ChildDirectories.Where(x => x.Exists && filterRegex.IsMatch(x.Name)).Cast<IDirectory>();
            }
            throw new NotImplementedException();
        }

        public IEnumerable<IFile> Files()
        {
            return ChildFiles.Where(x => x.Exists).Cast<IFile>();
        }

        public IEnumerable<IFile> Files(string filter, SearchScope searchScope)
        {
            if (searchScope == SearchScope.CurrentOnly)
            {
                var filterRegex = filter.Wildcard();
                return ChildFiles.Where(x => x.Exists && filterRegex.IsMatch(x.Name)).Cast<IFile>();
            }
            throw new NotImplementedException();
        }

        public IDirectory GetDirectory(string directoryName)
        {
            if (System.IO.Path.IsPathRooted(directoryName))
                return FileSystem.GetDirectory(directoryName);

            var inMemoryDirectory =
                    ChildDirectories.FirstOrDefault(x => x.Name.Equals(directoryName, _stringComparison));


            if (inMemoryDirectory == null)
            {
                inMemoryDirectory = new InMemoryDirectory(System.IO.Path.Combine(Path.FullPath, directoryName))
                {
                        Parent = this,
                        FileSystem = FileSystem
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
            linkDirectory.FileSystem = this.FileSystem;
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

        public IDirectory Create()
        {
            Exists = true;
            if (Parent != null && !Parent.Exists)
                Parent.Create();
            return this;
        }

        public override string ToString()
        {
            return Path.FullPath;
        }
    }
}