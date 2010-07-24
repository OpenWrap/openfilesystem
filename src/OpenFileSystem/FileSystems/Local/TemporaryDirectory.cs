using System;
using System.Collections.Generic;

namespace OpenFileSystem.IO.FileSystem.Local
{
    public class TemporaryDirectory : ITemporaryDirectory
    {
        public IDirectory UnderlyingDirectory { get; set; }

        public TemporaryDirectory(IDirectory unerlyingDirectory)
        {
            UnderlyingDirectory = unerlyingDirectory;
            if (!UnderlyingDirectory.Exists)
                UnderlyingDirectory.Create();
        }

        public IDirectory Create()
        {
            return UnderlyingDirectory.Create();
        }

        public IDirectory GetDirectory(string directoryName)
        {
            return UnderlyingDirectory.GetDirectory(directoryName);
        }

        public IFile GetFile(string fileName)
        {
            return UnderlyingDirectory.GetFile(fileName);
        }

        public IEnumerable<IFile> Files()
        {
            return UnderlyingDirectory.Files();
        }

        public IEnumerable<IDirectory> Directories()
        {
            return UnderlyingDirectory.Directories();
            
        }

        public IEnumerable<IFile> Files(string filter)
        {
            return UnderlyingDirectory.Files(filter);
            
        }

        public IEnumerable<IDirectory> Directories(string filter)
        {
            return UnderlyingDirectory.Directories(filter);
            
        }

        public void Add(IFile file)
        {
            UnderlyingDirectory.Add(file);

        }

        public bool IsHardLink
        {
            get { return UnderlyingDirectory.IsHardLink; }
        }

        public IDirectory LinkTo(string path)
        {
            return UnderlyingDirectory.LinkTo(path);
            
        }

        public IDirectory Target
        {
            get { return UnderlyingDirectory.Target; }
        }

        public IPath Path
        {
            get { return UnderlyingDirectory.Path; }
        }

        public IDirectory Parent
        {
            get { return UnderlyingDirectory.Parent; }
        }

        public IFileSystem FileSystem
        {
            get { return UnderlyingDirectory.FileSystem; }
        }

        public bool Exists
        {
            get { return UnderlyingDirectory.Exists; }
        }

        public string Name
        {
            get { return UnderlyingDirectory.Name; }
        }

        public void Delete()
        {
            UnderlyingDirectory.Delete();

        }
        

        ~TemporaryDirectory()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            Delete();
        }
    }

}