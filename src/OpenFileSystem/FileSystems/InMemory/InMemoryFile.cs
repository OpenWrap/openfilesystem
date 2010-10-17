using System;
using System.IO;
using OpenFileSystem.IO.FileSystem.Local;
using Path = OpenFileSystem.IO.FileSystem.Local.Path;

namespace OpenFileSystem.IO.FileSystem.InMemory
{
    public class InMemoryFile : IFile
    {
        void CopyFromFile(InMemoryFile fileToCopy)
        {
            Exists = true;
            LastModifiedTimeUtc = fileToCopy.LastModifiedTimeUtc;
            
            Stream = new FileStreamDouble(new MemoryStream(), FileStreamClosed);
            fileToCopy.Stream.Position = 0;
            fileToCopy.Stream.CopyTo(Stream);
            Stream.Position = 0;
        }

        public InMemoryFile(string filePath)
        {
            Path = new Path(filePath);
            Name = System.IO.Path.GetFileName(filePath);
            NameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(filePath);
            CreateNewStream();
            LastModifiedTimeUtc = DateTime.Now;
        }

        FileShare? _lock = null;
        void CreateNewStream()
        {
            Stream = new FileStreamDouble(new MemoryStream(), FileStreamClosed);
        }

        void FileStreamClosed()
        {
            _lock = null;
        }

        public FileStreamDouble Stream { get; set; }
        public IFile Create()
        {
            Exists = true;
            if (Parent != null && !Parent.Exists)
                Parent.Create();
            return this;
        }

        public void CopyTo(IFileSystemItem where)
        {
            if (where is InMemoryFile)
                ((InMemoryFile)where).CopyFromFile(this);
            if (where is InMemoryDirectory)
                ((InMemoryFile)((InMemoryDirectory)where).GetFile(Name)).CopyFromFile(this);
        }
        public void MoveTo(IFileSystemItem newFileName)
        {
            CopyTo(newFileName);
            Delete();
        }

        public Path Path { get; set; }
        public IDirectory Parent
        {
            get;
            set;
        }

        public IFileSystem FileSystem { get; set; }
        public bool Exists { get; set; }
        public string Name { get; private set; }
        public void Delete()
        {
            Exists = false;
        }

        public string NameWithoutExtension { get; private set; }

        public DateTime? LastModifiedTimeUtc
        {
            get;
            private set;
        }

        public Stream Open(FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
        {
            ValidateFileMode(fileMode);
            ValidateFileLock(fileAccess, fileShare);

            Stream.Position = 0;
            return Stream;
        }

        void ValidateFileLock(FileAccess fileAccess, FileShare fileShare)
        {
            // no previous lock, allow
            if (_lock == null)
            {
                _lock = fileShare;
                //if (fileAccess == FileAccess.Read)
                //    Stream.AsRead();
                //else if (fileAccess == FileAccess.Write)
                //    Stream.AsWrite();
                //else if (fileAccess == FileAccess.ReadWrite)
                Stream.AsReadWrite();
                return;
            }
            bool readAllowed = _lock.Value == FileShare.Read || _lock.Value == FileShare.ReadWrite;
            bool writeAllowed = _lock.Value == FileShare.Write || _lock.Value == FileShare.ReadWrite;

            if ((fileAccess == FileAccess.Read && !readAllowed) ||
                (fileAccess == FileAccess.ReadWrite && !(readAllowed && writeAllowed)) ||
                (fileAccess == FileAccess.Write && !writeAllowed))
                throw new IOException("File is locked. Please try again.");

        }

        void ValidateFileMode(FileMode fileMode)
        {
            if (Exists)
            {
                switch (fileMode)
                {
                    case FileMode.CreateNew:
                        throw new IOException("File already exists.");
                    case FileMode.Create:
                    case FileMode.Truncate:
                        CreateNewStream();
                        break;
                    case FileMode.Append:
                        Stream.Position = Stream.Length;
                        break;
                }
            }
            else
            {
                switch (fileMode)
                {
                    case FileMode.Append:
                    case FileMode.Create:
                    case FileMode.CreateNew:
                    case FileMode.OpenOrCreate:
                        Exists = true;
                        CreateNewStream();
                        break;
                    case FileMode.Open:
                    case FileMode.Truncate:
                        throw new FileNotFoundException();
                }
            }
        }
        public override string ToString()
        {
            return Path.FullPath;
        }
    }
}