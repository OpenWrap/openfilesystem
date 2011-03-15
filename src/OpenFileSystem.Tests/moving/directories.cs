using System;
using NUnit.Framework;
using OpenFileSystem.IO;
using OpenWrap.Testing;
using OpenWrap.Tests.IO;

namespace OpenFileSystem.Tests.moving
{
    [TestFixture(typeof(TestInMemoryFileSystem))]
    [TestFixture(typeof(TestLocalFileSystem))]
    class directories<T> : file_system_ctxt<T> where T : IFileSystem, new()
    {
        IDirectory _sourceDir;
        IDirectory _destDir;
        string _sourcePath;
        string _destPath;

        public directories()
        {
            var tempDir = given_temp_dir();
            _sourceDir = tempDir.GetDirectory("temp");
            _sourcePath = _sourceDir.Path.FullPath;
            _sourceDir.GetFile("test.txt").MustExist();

            _destDir = tempDir.GetDirectory("temp2");
            _destPath = _destDir.Path.FullPath;
            _sourceDir.MoveTo(_destDir);
        }

        [Test]
        public void original_doesnt_exist()
        {
            _sourceDir.Exists.ShouldBeFalse();
        }

        [Test]
        public void original_stays_at_original_path()
        {
            _sourceDir.Path.FullPath.ShouldBe(_sourcePath);

        }

        [Test]
        public void destination_exists()
        {
            _destDir.Exists.ShouldBeTrue();
        }

        [Test]
        public void destination_has_file()
        {
            _destDir.GetFile("test.txt").MustExist();
        }
    }
}
