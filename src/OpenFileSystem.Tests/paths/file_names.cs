using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenFileSystem.IO;
using OpenWrap.Testing;
using OpenWrap.Tests.IO;

namespace OpenFileSystem.Tests.paths
{
    
    [TestFixture(typeof(TestInMemoryFileSystem))]
    [TestFixture(typeof(TestLocalFileSystem))]
    public class standard_file_names<T> : file_system_ctxt<T> where T : IFileSystem, new()
    {
        public standard_file_names()
        {
            file = FileSystem.GetTempDirectory().GetFile("filename.first.txt");

        }

        [Test]
        public void name_contains_extension()
        {
            file.Name.ShouldBe("filename.first.txt");
        }

        [Test]
        public void name_without_extension_doesnt_contain_extension()
        {
            file.NameWithoutExtension.ShouldBe("filename.first");
        }

        [Test]
        public void extension_is_correct()
        {
            file.Extension.ShouldBe(".txt");
        }
        IFile file;
    }
}
