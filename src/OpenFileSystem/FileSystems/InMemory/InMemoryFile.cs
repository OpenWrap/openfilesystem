using System;
using System.IO;

namespace OpenFileSystem.IO.FileSystems.InMemory
{
    public class InMemoryFile : IFile
    {
        byte[] _content = new byte[4096];
        long _contentLength;
        readonly object _contentLock = new object();


        void CopyFromFile(InMemoryFile fileToCopy)
        {
            Exists = true;
            LastModifiedTimeUtc = fileToCopy.LastModifiedTimeUtc;
            lock (fileToCopy._contentLock)
            {
                var newContent = new byte[fileToCopy._content.Length];
                Buffer.BlockCopy(fileToCopy._content, 0, newContent, 0, fileToCopy._content.Length);
                _content = newContent;
                _contentLength = fileToCopy._contentLength;
            }
        }

        public InMemoryFile(string filePath)
        {
            Path = new Path(filePath);
            Name = System.IO.Path.GetFileName(filePath);
            NameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(filePath);
            
            LastModifiedTimeUtc = DateTime.Now;
        }

        FileShare? _lock = null;

        void FileStreamClosed()
        {
            _lock = null;
        }

        public IFile Create()
        {
            EnsureExists();
            return this;
        }

        void EnsureExists()
        {
            if (Exists) return;
            Exists = true;
            if (Parent != null && !Parent.Exists)
                Parent.Create();
            if (Parent != null)
            {
                ((InMemoryFileSystem)Parent.FileSystem).Notifier.NotifyCreation(this);
            }
        }

        public void CopyTo(IFileSystemItem where)
        {
            VerifyExists();
            if (where is InMemoryFile)
            {
                if (where.Exists)
                    throw new IOException("File already exists");
                ((InMemoryFile)where).CopyFromFile(this);
            }
            else if (where is InMemoryDirectory)
                ((InMemoryFile)((InMemoryDirectory)where).GetFile(Name)).CopyFromFile(this);
            else
                throw new InvalidOperationException("The target type doesn't match the file system of the current file.");
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
            EnsureExists();
            var beginPosition = ValidateFileMode(fileMode);
            ValidateFileLock(fileAccess, fileShare);
            return new FileStreamDouble(new InMemFileStream(this) { Position = beginPosition }, FileStreamClosed);
        }

        void VerifyExists()
        {
            if (Exists == false)
                throw new FileNotFoundException("File does not exist.", Path.ToString());
        }

        void ValidateFileLock(FileAccess fileAccess, FileShare fileShare)
        {
            if (_lock == null)
            {
                _lock = fileShare;
                return;
            }
            bool readAllowed = _lock.Value == FileShare.Read || _lock.Value == FileShare.ReadWrite;
            bool writeAllowed = _lock.Value == FileShare.Write || _lock.Value == FileShare.ReadWrite;

            if ((fileAccess == FileAccess.Read && !readAllowed) ||
                (fileAccess == FileAccess.ReadWrite && !(readAllowed && writeAllowed)) ||
                (fileAccess == FileAccess.Write && !writeAllowed))
                throw new IOException("File is locked. Please try again.");

        }

        long ValidateFileMode(FileMode fileMode)
        {
            if (Exists)
            {
                switch (fileMode)
                {
                    case FileMode.CreateNew:
                        throw new IOException("File already exists.");
                    case FileMode.Create:
                    case FileMode.Truncate:
                        _contentLength = 0;
                        return 0;
                    case FileMode.Open:
                    case FileMode.OpenOrCreate:
                        return 0;
                    case FileMode.Append:
                        return _contentLength;
                }
            }
            else
            {
                switch (fileMode)
                {
                    case FileMode.Create:
                    case FileMode.Append:
                    case FileMode.CreateNew:
                    case FileMode.OpenOrCreate:
                        EnsureExists();
                        return _contentLength;
                    case FileMode.Open:
                    case FileMode.Truncate:
                        throw new FileNotFoundException();
                }
            }
            throw new ArgumentException("fileMode not recognized.");
        }
        public override string ToString()
        {
            return Path.FullPath;
        }
        class InMemFileStream : Stream
        {
            readonly InMemoryFile _file;
            long _position = 0;

            public InMemFileStream(InMemoryFile file)
            {
                _file = file;
            }

            public override void Flush()
            {
            }

            public override long Seek(long offset, SeekOrigin origin)
            {

                if (origin == SeekOrigin.Begin)
                {
                    return MoveTo(offset);
                }
                if (origin == SeekOrigin.Current)
                {
                    return MoveTo(_position + offset);
                }
                if (origin == SeekOrigin.End)
                {
                    long size;
                    lock(_file._contentLock)
                        size = _file._contentLength;
                    return MoveTo(size - offset);
                }
                throw new ArgumentException("origin is not a recognized value");
            }

            long MoveTo(long offset)
            {
                if (offset < 0)
                    throw new ArgumentException("Cannot stream there");
                if (offset > _file._contentLength)
                    _file.Resize(offset);
                _position = offset;
                return offset;
            }

            public override void SetLength(long value)
            {
                _file.Resize(value);
            }

            public override int Read(byte[] buffer, int offset, int count)
            {

                var end = _position+count;
                var fileSize = _file._contentLength;
                long maxLengthToRead = end > fileSize ? fileSize - _position : count;
                Buffer.BlockCopy(_file._content, (int)_position, buffer, offset, (int)maxLengthToRead);
                _position += maxLengthToRead;
                return (int)maxLengthToRead;
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                var fileSize = _file._contentLength;

                var endOfWrite = (_position + count);
                if (endOfWrite > fileSize)
                    _file.Resize(endOfWrite);

                Buffer.BlockCopy(buffer, offset, _file._content, (int)_position, count);
                _position = _position + count;
            }

            public override bool CanRead
            {
                get { return true; }
            }

            public override bool CanSeek
            {
                get { return true; }
            }

            public override bool CanWrite
            {
                get { return true; }
            }

            public override long Length
            {
                get
                {
                    lock(_file._contentLock)
                        return _file._contentLength;
                }
            }

            public override long Position
            {
                get { return _position; }
                set { 
                    Seek(value, SeekOrigin.Begin);
                }
            }
        }

        void Resize(long offset)
        {
            lock(_contentLock)
            {
                if (_contentLength < offset)
                    _contentLength = offset;
                if (_content.Length < _contentLength)
                {
                    var buffer = new byte[_content.Length * 2];
                    Buffer.BlockCopy(_content, 0, buffer, 0, _content.Length);
                    _content = buffer;
                }
            }
        }
    }
}