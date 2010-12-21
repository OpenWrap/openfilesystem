using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenFileSystem.IO;
using OpenWrap.Testing;
using OpenWrap.Tests.IO;

namespace OpenFileSystem.Tests.writing_content
{

    [TestFixture(typeof(TestInMemoryFileSystem))]
    [TestFixture(typeof(TestLocalFileSystem))]
    public class using_append<T> : file_system_ctxt<T> where T : IFileSystem, new()
    {
        public using_append()
        {
            file = given_temp_file();
            given_content(1);
            given_content(42);
            
        }

        void given_content(byte data)
        {
            file.Write(data, mode: FileMode.Append);
        }

        [Test]
        public void file_length_is_updated()
        {
            file.Size.ShouldBe(2);
        }

        [Test]
        public void file_content_is_written()
        {
            file.ShouldBe(1, 42);
        }
        ITemporaryFile file;
    }
}
