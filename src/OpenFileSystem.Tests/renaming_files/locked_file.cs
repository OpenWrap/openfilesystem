using System;
using System.IO;
using NUnit.Framework;
using OpenFileSystem.IO;
using contexts;

namespace renaming_files
{
    [TestFixture(typeof(TestInMemoryFileSystem))]
    [TestFixture(typeof(TestLocalFileSystem))]
    public class locked_file<T> : files<T> where T : IFileSystem, new()
    {
        [TestCase(FileShare.None)]
        [TestCase(FileShare.Read)]
        [TestCase(FileShare.ReadWrite)]
        [TestCase(FileShare.Write)]
        public void cannot_be_moved(FileShare fileShare)
        {
            var tempFile = FileSystem.CreateTempFile();
            using(var openStream = tempFile.Open(FileMode.Append, FileAccess.Write, fileShare))
            {
                Executing(() => tempFile.MoveTo(tempFile.Parent.GetFile(Guid.NewGuid().ToString())))
                    .ShouldThrow<IOException>();
            }
        }
}
    }
