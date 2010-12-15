﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenFileSystem.IO.FileSystems.Local
{
    public abstract class LocalDirectory : AbstractDirectory, IDirectory, IEquatable<IDirectory>
    {
        DirectoryInfo DirectoryInfo { get; set; }

        protected LocalDirectory(DirectoryInfo directory)
        {
            DirectoryInfo = directory;
            Path = new Path(NormalizeDirectoryPath(DirectoryInfo.FullName));
        }

        protected LocalDirectory(string directoryPath)
            : this(new DirectoryInfo(directoryPath))
        {
        }

        protected abstract LocalDirectory CreateDirectory(DirectoryInfo di);
        protected abstract LocalDirectory CreateDirectory(string path);

        public bool Exists
        {
            get
            {
                DirectoryInfo.Refresh();
                return DirectoryInfo.Exists;
            }
        }

        public override string ToString()
        {
            return Path.FullPath;
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

        public Path Path
        {
            get;
            private set;
        }

        public void Add(IFile file)
        {
            File.Copy(file.Path.FullPath, System.IO.Path.Combine(DirectoryInfo.FullName, file.Name));
        }

        public virtual bool IsHardLink { get { return false; } }

        public abstract IDirectory LinkTo(string path);

        public virtual IDirectory Target
        {
            get
            {
                return this;
            }
        }
        
        public IDisposable FileChanges(string filter = "*", bool includeSubdirectories = false, Action<IFile> created = null, Action<IFile> modified = null, Action<IFile> deleted = null, Action<IFile> renamed = null)
        {
            return LocalFileSystemNotifier.Instance.RegisterNotification(Path, filter, includeSubdirectories, created, modified, deleted, renamed);
        }

        public virtual IEnumerable<IDirectory> Directories(string filter, SearchScope scope)
        {
            DirectoryInfo.Refresh();
            return DirectoryInfo.GetDirectories(filter, scope == SearchScope.CurrentOnly ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories)
                .Select(x => (IDirectory)CreateDirectory(x));
        }

        public IEnumerable<IFile> Files()
        {
            DirectoryInfo.Refresh();
            return DirectoryInfo.GetFiles().Select(x => (IFile)new LocalFile(x.FullName, CreateDirectory));
        }

        public IEnumerable<IFile> Files(string filter, SearchScope scope)
        {
            DirectoryInfo.Refresh();
            return DirectoryInfo.GetFiles(filter, scope == SearchScope.CurrentOnly ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories)
                                .Select(x => new LocalFile(x.FullName, CreateDirectory))
                                .Cast<IFile>();
        }

        public virtual IDirectory GetDirectory(string directoryName)
        {
            return CreateDirectory(System.IO.Path.Combine(DirectoryInfo.FullName, directoryName));
        }

        public IFile GetFile(string fileName)
        {
            return new LocalFile(System.IO.Path.Combine(DirectoryInfo.FullName, fileName), CreateDirectory);
        }

        public virtual IEnumerable<IDirectory> Directories()
        {
            DirectoryInfo.Refresh();
            return DirectoryInfo.GetDirectories().Select(x => (IDirectory)CreateDirectory(x));
        }

        public virtual void Delete()
        {
            DirectoryInfo.Refresh();
            if (DirectoryInfo.Exists)
                DirectoryInfo.Delete(true);
        }

        public virtual IDirectory Create()
        {
            DirectoryInfo.Refresh();
            DirectoryInfo.Create();
            return this;
        }

        public void MoveTo(IFileSystemItem newFileName)
        {
            DirectoryInfo.Refresh();
            DirectoryInfo.MoveTo(newFileName.Path.FullPath);
        }
        public void CopyTo(IFileSystemItem newItem)
        {
            IDirectory destDir = (IDirectory)newItem;

            DirectoryInfo.Refresh();
            if (!DirectoryInfo.Exists)
                DirectoryInfo.Create();
            foreach (var file in Files())
                file.CopyTo(newItem);
            foreach(var directory in Directories())
                directory.CopyTo(destDir.GetDirectory(directory.Name).MustExist());
            DirectoryInfo.Refresh();
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
            return Path.GetHashCode();
        }

    }
}