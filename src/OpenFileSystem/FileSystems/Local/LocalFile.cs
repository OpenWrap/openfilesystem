using System;
using System.IO;

namespace OpenFileSystem.IO.FileSystem.Local
{
    public class LocalFile : IFile, IEquatable<LocalFile>
    {
        public bool Equals(LocalFile other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Path, Path);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(LocalFile)) return false;
            return Equals((LocalFile)obj);
        }

        public override int GetHashCode()
        {
            return Path.GetHashCode();
        }

        public static bool operator ==(LocalFile left, LocalFile right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(LocalFile left, LocalFile right)
        {
            return !Equals(left, right);
        }

        readonly string _filePath;
        readonly Func<DirectoryInfo, IDirectory> _directoryFactory;

        public LocalFile(string filePath, Func<DirectoryInfo, IDirectory> directoryFactory)
        {
            _filePath = filePath;
            _directoryFactory = directoryFactory;
            Path = new LocalPath(filePath);
        }

        public bool Exists
        {
            get { return File.Exists(_filePath); }
        }

        public IFileSystem FileSystem
        {
            get { return LocalFileSystem.Instance; }
        }

        public string Name
        {
            get { return System.IO.Path.GetFileName(_filePath); }
        }

        public string NameWithoutExtension
        {
            get { return System.IO.Path.GetFileNameWithoutExtension(_filePath); }
        }

        public DateTime? LastModifiedTimeUtc
        {
            get { return Exists ? new FileInfo(_filePath).LastWriteTimeUtc : (DateTime?)null; }
        }

        public IDirectory Parent
        {
            get
            {
                try
                {
                    var directoryInfo = Directory.GetParent(_filePath);
                    return directoryInfo == null
                               ? null
                               : _directoryFactory(directoryInfo);
                }
                catch (DirectoryNotFoundException)
                {
                    return null;
                }
            }
        }

        public IPath Path { get; private set; }

        public Stream Open(FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
        {
            if (!Exists)
                Create();
            return File.Open(_filePath, fileMode, fileAccess, fileShare);
        }

        public void Delete()
        {
            File.Delete(_filePath);
        }

        public IFile Create()
        {
            // creates the parent if it doesnt exist
            if (!Parent.Exists)
                Parent.Create();

            File.Create(Path.FullPath).Close();
            return this;
        }
    }
}