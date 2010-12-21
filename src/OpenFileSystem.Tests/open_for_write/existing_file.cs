﻿using System.IO;
using NUnit.Framework;
using OpenFileSystem.IO;
using OpenWrap.Testing;
using OpenWrap.Tests.IO;

namespace OpenFileSystem.Tests.open_for_write
{
    [TestFixture(typeof(TestInMemoryFileSystem))]
    [TestFixture(typeof(TestLocalFileSystem))]
    public class existing_file<T> : context<T> where T : IFileSystem, new()
    {
        IFile file;

        public existing_file()
        {
            given_temp_dir();
        }


        [Test]
        public void file_is_appended()
        {
            file = write_to_file();

            file.Write(1, mode: FileMode.Append);

            file.ShouldBe(0, 1);
        }

        [TestCase(FileMode.Truncate)]
        [TestCase(FileMode.Create)]
        public void file_is_truncated_for(FileMode fileMode)
        {

            file = write_to_file();

            file.Write(1, fileMode);

            file.ShouldBe(1);
        }
        [Test]
        public void file_is_edited()
        {
            file = write_to_file(new byte[] { 0, 1 });
            file.Write(2, FileMode.Open);
            file.ShouldBe(2, 1);
        }
        [Test]
        public void file_cannot_be_created_new()
        {

            file = write_to_file();
            Executing(() => file.Write(1, mode: FileMode.CreateNew))
                .ShouldThrow<IOException>();
        }

    }
}