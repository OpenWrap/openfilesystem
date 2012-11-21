using System;
using System.IO;
using NUnit.Framework;
using OpenFileSystem.IO;
using contexts;

namespace copying_files
{
    [TestFixture(typeof(TestInMemoryFileSystem))]
    [TestFixture(typeof(TestLocalFileSystem))]
    public class locked_file<T> : files<T> where T : IFileSystem, new()
    {
        [TestCase(FileShare.None)]
        [TestCase(FileShare.Write)]
        [TestCase(FileShare.Delete)]
        public void cannot_be_copied(FileShare fileShare)
        {
            var tempFile = FileSystem.CreateTempFile();
            using(tempFile.Open(FileMode.Append, FileAccess.Write, fileShare))
            {
                Executing(() => tempFile.CopyTo(tempFile.Parent.GetFile(Guid.NewGuid().ToString())))
                    .ShouldThrow<IOException>();
            }
        }
}
    }
