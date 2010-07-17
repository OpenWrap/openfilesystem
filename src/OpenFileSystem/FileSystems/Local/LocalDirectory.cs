using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenFileSystem.IO.FileSystem.Local
{
    public class LocalDirectory : AbstractDirectory, IDirectory, IEquatable<IDirectory>
    {
        protected DirectoryInfo DirectoryInfo { get; private set; }

        public LocalDirectory(DirectoryInfo directory)
        {
            DirectoryInfo = directory;
            Path = new LocalPath(NormalizeDirectoryPath(DirectoryInfo.FullName));
        }

        public LocalDirectory(string directoryPath)
            : this(new DirectoryInfo(directoryPath))
        {
        }
        protected virtual LocalDirectory CreateDirectory(DirectoryInfo di)
        {
            return new LocalDirectory(di);
        }
        protected virtual LocalDirectory CreateDirectory(string path)
        {
            return new LocalDirectory(path);
        }
        public bool Exists
        {
            get { return DirectoryInfo.Exists; }
        }

        public IFileSystem FileSystem
        {
            get { return LocalFileSystem.Instance; }
        }

        public string Name
        {
            get { return DirectoryInfo.Name; }
        }

        public virtual IDirectory Parent
        {
            get { return DirectoryInfo.Parent == null ? null : CreateDirectory(DirectoryInfo.Parent); }
        }

        public IPath Path
        {
            get; private set;
        }

        public void Add(IFile file)
        {
            File.Copy(file.Path.FullPath, System.IO.Path.Combine(DirectoryInfo.FullName, file.Name));
        }

        public virtual bool IsHardLink { get { return false; } }

        public virtual IDirectory LinkTo(string path)
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<IDirectory> Directories(string filter)
        {
            return DirectoryInfo.GetDirectories(filter).Select(x => (IDirectory)CreateDirectory(x));
        }

        public IEnumerable<IFile> Files()
        {
            return DirectoryInfo.GetFiles().Select(x => (IFile)new LocalFile(x.FullName));
        }

        public IEnumerable<IFile> Files(string filter)
        {
            return DirectoryInfo.GetFiles(filter).Select(x => (IFile)new LocalFile(x.FullName));
        }

        public virtual IDirectory GetDirectory(string directoryName)
        {
            return CreateDirectory(System.IO.Path.Combine(DirectoryInfo.FullName, directoryName));
        }

        public IFile GetFile(string fileName)
        {
            return new LocalFile(System.IO.Path.Combine(DirectoryInfo.FullName, fileName));
        }

        public virtual IEnumerable<IDirectory> Directories()
        {
            return DirectoryInfo.GetDirectories().Select(x => (IDirectory)CreateDirectory(x));
        }

        public virtual void Delete()
        {
            if (DirectoryInfo.Exists)
                DirectoryInfo.Delete(true);
        }

        public virtual IDirectory Create()
        {
            DirectoryInfo.Create();
            return this;
        }

        public bool Equals(IDirectory other)
        {
            if (ReferenceEquals(null, other)) return false;
            return other.Path.Equals(Path);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj as IDirectory == null) return false;
            return Equals((IDirectory)obj);
        }

        public override int GetHashCode()
        {
            return 0;
        }

    }
}