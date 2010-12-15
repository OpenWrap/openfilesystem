﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems;
using OpenFileSystem.IO.FileSystems.InMemory;
using OpenFileSystem.IO.FileSystems.Local;
using OpenWrap.Testing;
using Path = OpenFileSystem.IO.Path;

namespace OpenWrap.Tests.IO
{
    public class TestInMemoryFileSystem : InMemoryFileSystem
    {
        public TestInMemoryFileSystem()
        {
            this.CurrentDirectory = Environment.CurrentDirectory;
        }
    }
    [TestFixture(typeof(TestInMemoryFileSystem))]
    [TestFixture(typeof(TestLocalFileSystem))]
    public class file_system<T> : context where T : IFileSystem, new()
    {
        public file_system()
        {
            CurrentDirectory = Environment.CurrentDirectory;
            FileSystem = new T();
        }
        [Test]
        public void receives_creation_notification()
        {
            using (var tempDir = FileSystem.CreateTempDirectory())
            {
                string filePath = null;
                using (tempDir.FileChanges(created: f => filePath = f.Path.FullPath))
                {

                    tempDir.GetFile("hello.txt").MustExist();
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                    filePath.ShouldBe(tempDir.Path.Combine("hello.txt").FullPath);
                }
            }
        }
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
            var directory = FileSystem.GetDirectory(@"C:\test\test.html");

            directory.Exists.ShouldBeFalse();
        }
        [Test]
        public void created_directory_exists()
        {
            var directory = FileSystem.CreateDirectory(@"C:\test\temp.html");

            directory.Exists.ShouldBeTrue();
        }

        [Test]
        public void can_get_subdirectory_of_non_existant_directory()
        {
            FileSystem.GetDirectory(@"C:\mordor").GetDirectory(@"shire\galladrin")
                .Path.FullPath.ShouldBe(@"C:\mordor\shire\galladrin\");
        }

        [Test]
        public void can_get_file_with_directory_path()
        {
            FileSystem.GetDirectory(@"C:\mordor").GetFile(@"shire\frodo.txt")
                .Path.FullPath.ShouldBe(@"C:\mordor\shire\frodo.txt");
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
            var file = FileSystem.GetFile(@"C:\mordor\elves.txt");
            file.Exists.ShouldBeFalse();
            file.OpenWrite().Close();
            file.Exists.ShouldBeTrue();
            file.Delete();
        }
        [Test]
        public void file_paths_are_normalized()
        {
            var file = FileSystem.GetFile(@"C:\\folder\\file");
            file.Path.FullPath.ShouldBe(@"C:\folder\file");
        }

        [Test]
        public void trailing_slash_is_not_significant()
        {
            var first = FileSystem.GetDirectory(@"C:\mordor");
            var second = FileSystem.GetDirectory(@"C:\mordor\");

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
            FileSystem.GetDirectory(@"C:\tmp\1\").ShouldNotBe(FileSystem.GetDirectory(@"C:\tmp\2\"));

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
            using (var temporaryFile = given_temp_file("Hello"))
            using (var writer = temporaryFile.Open(FileMode.Truncate, FileAccess.Write, FileShare.None))
                writer.Length.ShouldBe(0);
        }
        [Test]
        public void open_write_with_create_creates_a_new_stream()
        {
            using (var temporaryFile = given_temp_file("Hello"))
            using (var writer = temporaryFile.Open(FileMode.Create, FileAccess.Write, FileShare.None))
                writer.Length.ShouldBe(0);
        }
        [Test]
        public void copy_files_copies_content()
        {
            using (var tempDir = FileSystem.CreateTempDirectory())
            {
                var tempFile = tempDir.GetFile("test.txt");
                WriteString(tempFile, "test data");
                var copyFile = tempDir.GetFile("test2.txt");
                tempFile.CopyTo(copyFile);

                ReadString(tempFile).ShouldBe("test data");
                ReadString(copyFile).ShouldBe("test data");

            }
        }
        [Test]
        public void can_read_data_from_two_readers()
        {
            using(var file = given_temp_file("content"))
            {
                using(var first = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
                using(var second = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    first.ReadByte().ShouldBe('c');
                    second.ReadByte().ShouldBe('c');
                }
            }
        }

        [Test]
        public void duplicates_directory_content_when_copyto_called()
        {
            using(var source = FileSystem.CreateTempDirectory())
            using (var destination = FileSystem.CreateTempDirectory())
            {
                WriteString(source.GetFile("test.txt"), "test data");
                WriteString(source.GetDirectory("testDir").GetFile("test2.txt"), "test data2");

                source.CopyTo(destination);

                destination.GetFile("test.txt")
                    .Check(x=>x.Exists.ShouldBeTrue())
                    .Check(x=> ReadString(x).ShouldBe("test data"));
                var dest = destination.GetDirectory("testDir");

                dest.Exists.ShouldBeTrue();
                var destFile = dest.GetFile("test2.txt");
                destFile.Exists.ShouldBeTrue();
                ReadString(destFile).ShouldBe("test data2");

            }
        }
        protected ITemporaryFile given_temp_file(string content)
        {
            var temporaryFile = FileSystem.CreateTempFile();
            WriteString(temporaryFile, content);

            return temporaryFile;
        }
        string ReadString(IFile file)
        {
            using (var reader = file.OpenRead())
            {
                var stream = new MemoryStream();
                reader.CopyTo(stream);

                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        void WriteString(IFile temporaryFile, string content)
        {
            using (var writer = temporaryFile.OpenWrite())
                writer.Write(Encoding.UTF8.GetBytes(content));
        }


        protected IFileSystem FileSystem { get; private set; }
        protected string CurrentDirectory { get; set; }
    }

    public class TestLocalFileSystem : IFileSystem
    {
        public IDirectory GetDirectory(string directoryPath)
        {
            return _local.GetDirectory(directoryPath);
        }

        public Path GetPath(string path)
        {
            return _local.GetPath(path);
        }

        public ITemporaryDirectory CreateTempDirectory()
        {
            return _local.CreateTempDirectory();
        }

        public IDirectory CreateDirectory(string path)
        {
            return _local.CreateDirectory(path);
        }

        public IFile GetFile(string itemSpec)
        {
            return _local.GetFile(itemSpec);
        }

        public ITemporaryFile CreateTempFile()
        {
            return _local.CreateTempFile();
        }

        public IDirectory GetTempDirectory()
        {
            return _local.GetTempDirectory();
        }

        IFileSystem _local = LocalFileSystem.Instance;
    }

    public class path_specification : context
    {
        [Test]
        public void path_has_segments()
        {
            var path = new Path(@"C:\mordor\nurn");
            path.Segments.ShouldHaveSameElementsAs(new[] { @"C:", "mordor", "nurn" });
        }
        [Test]
        public void trailing_slash_is_always_normalized()
        {
            new Path(@"C:\mordor\nurn").ShouldBe(new Path(@"C:\mordor\nurn\"));
        }
    }
}
