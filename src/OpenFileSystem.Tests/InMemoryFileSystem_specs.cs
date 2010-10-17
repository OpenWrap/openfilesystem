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
        [Test]
        public void paths_are_correct()
        {
            var path = @"c:\tmp\TestPackage-1.0.0.1234.wrap";
            var file = FileSystem.GetFile(path);
            file.ToString().ShouldBe(path);
        }

        [Test]
        public void can_add_folders_to_fs()
        {
            var fs = new InMemoryFileSystem(new InMemoryDirectory(@"c:\mordor"));
            fs.Directories.ShouldHaveCountOf(1);
        }
        [Test]
        public void can_add_sub_folders()
        {
            var fs = new InMemoryFileSystem(new InMemoryDirectory(@"c:\mordor\nurn"));
            var mordor = fs.GetDirectory(@"c:\mordor");
            mordor.Exists.ShouldBeTrue();

            var nurn = mordor.GetDirectory("nurn");
            nurn.Path.FullPath.ShouldBe(@"c:\mordor\nurn\");
            nurn.Exists.ShouldBeTrue();
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
