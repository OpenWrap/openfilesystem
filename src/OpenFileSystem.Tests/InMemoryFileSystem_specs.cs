using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystem.InMemory;
using OpenWrap.Testing;

namespace OpenFileSystem.Tests
{
    public class in_mem_specification : context.in_memory_file_system
    {
        [Test]
        public void folders_and_paths_are_case_insensitive_by_default()
        {
            given_directory(@"c:\test");
            given_directory(@"C:\TEST\test2");

            FileSystem.GetDirectory(@"C:\TEST")
                      .ShouldBeTheSameInstanceAs(FileSystem.GetDirectory(@"c:\test"));
        }
    }
    namespace context
    {
        public class in_memory_file_system : OpenWrap.Testing.context
        {
            public in_memory_file_system()
            {
                FileSystem = new InMemoryFileSystem();
            }

            protected InMemoryFileSystem FileSystem { get; set; }

            protected void given_directory(string cTest)
            {
                FileSystem.GetDirectory(cTest).MustExist();
            }
        }
    }
}
