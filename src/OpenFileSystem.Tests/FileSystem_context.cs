using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystem.InMemory;
using OpenFileSystem.IO.FileSystem.Local;
using OpenFileSystem.IO.FileSystems;
using OpenWrap.Testing;
using Path = OpenFileSystem.IO.FileSystem.Local.Path;

namespace OpenWrap.Tests.IO
{

    public abstract class file_system<T> : context where T : IFileSystem
    {
        [Test]
        public void temp_file_exists_after_creation_and_is_deleted_when_used()
        {
            string fullPath;
            using (var tempFile = FileSystem.CreateTempFile())
            {
                tempFile.Exists.ShouldBeTrue();
                fullPath = tempFile.Path.FullPath;
                var tempFile2 = FileSystem.GetFile(fullPath);
                tempFile2.Exists.ShouldBeTrue();
                tempFile2.ShouldBe(tempFile);
            }
            FileSystem.GetFile(fullPath).Exists.ShouldBeFalse();
        }

        [Test]
        public void temp_directory_is_rooted_correctly()
        {
            using (var tempDirectory = FileSystem.CreateTempDirectory())
            {
                tempDirectory.Parent.ShouldBe(FileSystem.GetTempDirectory());
            }
        }
        [Test]
        public void directories_always_created()
        {
            var directory = FileSystem.GetDirectory(@"c:\test\test.html");

            directory.Exists.ShouldBeFalse();
        }
        [Test]
        public void created_directory_exists()
        {
            var directory = FileSystem.CreateDirectory(@"c:\test\temp.html");

            directory.Exists.ShouldBeTrue();
        }

        [Test]
        public void can_get_subdirectory_of_non_existant_directory()
        {
            FileSystem.GetDirectory(@"c:\mordor").GetDirectory(@"shire\galladrin")
                .Path.FullPath.ShouldBe(@"c:\mordor\shire\galladrin\");
        }

        [Test]
        public void can_get_file_with_directory_path()
        {
            FileSystem.GetDirectory(@"c:\mordor").GetFile(@"shire\frodo.txt")
                .Path.FullPath.ShouldBe(@"c:\mordor\shire\frodo.txt");
        }
        [Test]
        public void directory_is_resolved_relative_to_current_directory()
        {
            var dir = FileSystem.GetDirectory("shire");

            dir.Path.FullPath.ShouldBe(System.IO.Path.Combine(CurrentDirectory, @"shire\"));
            dir.Exists.ShouldBeFalse();
        }
        [Test]
        public void files_are_resolved_relative_to_current_directory()
        {
            FileSystem.GetFile("rohan.html").Path.FullPath
                .ShouldBe(System.IO.Path.Combine(CurrentDirectory, "rohan.html"));
        }

        [Test]
        public void recursive_search_for_directories_returns_correct_directory()
        {
            using (var tempDirectory = FileSystem.CreateTempDirectory())
            {
                var mordor = tempDirectory.GetDirectory("mordor");
                mordor.GetDirectory("shire").MustExist();
                mordor.GetDirectory("rohan").MustExist();

                tempDirectory.Directories("s*", SearchScope.SubFolders).ShouldHaveCountOf(1)
                    .First().Name.ShouldBe("shire");
            }
        }
        [Test]
        public void recursive_search_for_files_returns_correct_files()
        {
            using (var tempDirectory = FileSystem.CreateTempDirectory())
            {
                var mordor = tempDirectory.GetDirectory("mordor");
                mordor.GetFile("shire").MustExist();
                mordor.GetFile("rohan").MustExist();

                tempDirectory.Files("s*", SearchScope.SubFolders).ShouldHaveCountOf(1)
                    .First().Name.ShouldBe("shire");
            }
        }
        [Test]
        public void two_directories_are_equal()
        {
            FileSystem.GetDirectory("shire").ShouldBe(FileSystem.GetDirectory("shire"));
        }
        [Test]
        public void two_files_are_equal()
        {
            FileSystem.GetDirectory("rohan.html").ShouldBe(FileSystem.GetDirectory("rohan.html"));
        }
        [Test]
        public void non_existant_file_opened_for_write_is_created_automatically()
        {
            var file = FileSystem.GetFile(@"c:\mordor\elves.txt");
            file.Exists.ShouldBeFalse();
            file.OpenWrite().Close();
            file.Exists.ShouldBeTrue();
            file.Delete();
        }
        [Test]
        public void file_paths_are_normalized()
        {
            var file = FileSystem.GetFile(@"c:\\folder\\file");
            file.Path.FullPath.ShouldBe(@"c:\folder\file");
        }

        [Test]
        public void trailing_slash_is_not_significant()
        {
            var first = FileSystem.GetDirectory(@"c:\mordor");
            var second = FileSystem.GetDirectory(@"c:\mordor\");

            first.ShouldBe(second);
        }
        [Test]
        public void standard_directory_is_not_a_link()
        {
            using (var dir = FileSystem.CreateTempDirectory())
            {
                dir.IsHardLink.ShouldBeFalse();
            }
        }
        [Test]
        public void can_create_link()
        {
            var tempLinkFolder = Guid.NewGuid().ToString();
            using (var concreteDir = FileSystem.CreateTempDirectory())
            {
                concreteDir.GetFile("test.txt").OpenWrite().Close();
                string linkedPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), tempLinkFolder);
                var linkedDirectory = concreteDir.LinkTo(linkedPath);
                linkedDirectory.IsHardLink.ShouldBeTrue();

                linkedDirectory.Delete();
                linkedDirectory.Exists.ShouldBeFalse();
                concreteDir.Exists.ShouldBeTrue();
                concreteDir.GetFile("test.txt").Exists.ShouldBeTrue();
            }
        }
        [Test]
        public void different_directories_are_not_equal()
        {
            FileSystem.GetDirectory(@"c:\tmp\1\").ShouldNotBe(FileSystem.GetDirectory(@"c:\tmp\2\"));

        }
        [Test]
        public void link_has_reference_to_target()
        {
            using (var tempDir = FileSystem.CreateTempDirectory())
            {
                var concreteDir = tempDir.GetDirectory("temp").MustExist();
                var linkedDir = concreteDir.LinkTo(tempDir.Path.Combine("link").FullPath);
                linkedDir.Target.ShouldBe(concreteDir);
                linkedDir.Delete();

            }
        }

        [Test]
        public void delete_parent_deletes_child_folder()
        {
            var dir1 = FileSystem.GetTempDirectory().GetDirectory("test");
            var child = dir1.GetDirectory("test").MustExist();
            dir1.Delete();
            dir1.Exists.ShouldBeFalse();
            child.Exists.ShouldBeFalse();
        }
        [Test]
        public void deleted_child_directory_doesnt_show_up_in_child_directories()
        {
            var dir1 = FileSystem.GetTempDirectory().GetDirectory("test");
            var child = dir1.GetDirectory("test").MustExist();
            child.Delete();

            dir1.Directories().ShouldHaveCountOf(0);

        }
        [Test]
        public void deleted_hardlink_doesnt_delete_subfolder()
        {
            var dir1 = FileSystem.GetTempDirectory().GetDirectory("test").MustExist();
            var child = dir1.GetDirectory("test").MustExist();

            var link = dir1.LinkTo(FileSystem.GetTempDirectory().Path.Combine("testLink").FullPath);
            link.Delete();
            link.Exists.ShouldBeFalse();
            dir1.Exists.ShouldBeTrue();
            child.Exists.ShouldBeTrue();
        }
        [Test]
        public void moving_file_moves_a_file()
        {
            var temporaryFile = FileSystem.CreateTempFile();
            var newFileName = FileSystem.GetTempDirectory().GetFile(Guid.NewGuid().ToString());
            temporaryFile.MoveTo(newFileName);

            FileSystem.GetFile(temporaryFile.Path.FullPath).Exists.ShouldBeFalse();
            newFileName.Exists.ShouldBeTrue();

        }
        [Test]
        public void moving_directory_moves_directories()
        {
            var tempDirectory = FileSystem.CreateTempDirectory();
            var oldDirName = tempDirectory.Path.FullPath;

            var newDir = FileSystem.GetDirectory(FileSystem.GetTempDirectory().Path.Combine(Guid.NewGuid().ToString() + "/").FullPath);
            tempDirectory.MoveTo(newDir);
            newDir.Exists.ShouldBeTrue();
            FileSystem.GetDirectory(oldDirName).Exists.ShouldBeFalse();
        }
        [TestCase(FileAccess.Read, FileShare.None, FileAccess.Read)]
        [TestCase(FileAccess.Read, FileShare.Write, FileAccess.Read)]
        [TestCase(FileAccess.Write, FileShare.None, FileAccess.Write)]
        [TestCase(FileAccess.Write, FileShare.Read, FileAccess.Write)]
        [TestCase(FileAccess.Read, FileShare.Read, FileAccess.Write)]
        public void failing_locks(FileAccess firstAccess, FileShare @lock, FileAccess nextAccess)
        {
            using (var temporaryFile = FileSystem.CreateTempFile())
            using (var stream1 = temporaryFile.Open(FileMode.OpenOrCreate, firstAccess, @lock))
                Executing(() => temporaryFile.Open(FileMode.OpenOrCreate, nextAccess, FileShare.None))
                   .ShouldThrow<IOException>();
        }
        [Test]
        public void open_write_with_truncate_creates_a_new_stream()
        {
            using (var temporaryFile = given_file("Hello"))
            using (var writer = temporaryFile.Open(FileMode.Truncate, FileAccess.Write, FileShare.None))
                writer.Length.ShouldBe(0);
        }
        [Test]
        public void open_write_with_create_creates_a_new_stream()
        {
            using (var temporaryFile = given_file("Hello"))
            using (var writer = temporaryFile.Open(FileMode.Create, FileAccess.Write, FileShare.None))
                writer.Length.ShouldBe(0);
        }
        [Test]
        protected ITemporaryFile given_file(string content)
        {
            var temporaryFile = FileSystem.CreateTempFile();
            using (var writer = temporaryFile.OpenWrite())
                writer.Write(Encoding.UTF8.GetBytes("Hello"));

            return temporaryFile;
        }

        protected IFileSystem FileSystem { get; set; }
        protected string CurrentDirectory { get; set; }
    }

    public class in_memory_fs : file_system<InMemoryFileSystem>
    {
        [SetUp]
        public void setup()
        {
            CurrentDirectory = @"c:\mordor";
            FileSystem = new InMemoryFileSystem(
                new InMemoryDirectory(@"c:\mordor",
                    new InMemoryFile("rings.txt")
                )
            )
            {
                CurrentDirectory = CurrentDirectory
            };
        }
    }

    public class local_fs : file_system<LocalFileSystem>
    {
        [SetUp]
        public void setup()
        {
            CurrentDirectory = Environment.CurrentDirectory;

            FileSystem = LocalFileSystem.Instance;
        }
    }
    public class path_specification : context
    {
        [Test]
        public void path_has_segments()
        {
            var path = new Path(@"c:\mordor\nurn");
            path.Segments.ShouldHaveSameElementsAs(new[] { @"c:", "mordor", "nurn" });
        }
        [Test]
        public void trailing_slash_is_always_normalized()
        {
            new Path(@"c:\mordor\nurn").ShouldBe(new Path(@"c:\mordor\nurn\"));
        }
    }
}
