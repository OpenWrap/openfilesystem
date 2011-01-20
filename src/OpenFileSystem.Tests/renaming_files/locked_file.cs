using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenFileSystem.IO;
using OpenFileSystem.Tests.contexts;
using OpenWrap.Testing;
using OpenWrap.Tests.IO;

namespace OpenFileSystem.Tests.renaming_files
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
