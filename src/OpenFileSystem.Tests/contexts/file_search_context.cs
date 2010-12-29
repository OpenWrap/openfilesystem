using System;
using System.Collections.Generic;
using NUnit.Framework;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems.InMemory;

namespace OpenFileSystem.Tests.contexts
{
        public abstract class file_search_context : OpenWrap.Testing.context
        {
            protected IFileSystem FileSystem;
            protected IEnumerable<IFile> Files;
            protected IEnumerable<IDirectory> Directories;

            public file_search_context()
            {
                FileSystem = new InMemoryFileSystem();
            }
            protected void given_file(string filePath)
            {
                FileSystem.GetFile(filePath).MustExist();
            }

            protected void when_searching_for_files(string searchSpec)
            {
                Files = FileSystem.Files(searchSpec);
            }
            protected void when_searching_for_directories(string searchSpec)
            {
                Directories = FileSystem.Directories(searchSpec);
            }

            protected void given_directory(string directory)
            {
                FileSystem.GetDirectory(directory).MustExist();
            }
        }
}
