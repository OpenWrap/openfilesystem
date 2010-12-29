using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems.InMemory;
using OpenFileSystem.Tests.open_for_write;
using OpenWrap.Testing;
using OpenWrap.Tests.IO;

namespace OpenFileSystem.Tests.directory_searches
{
    [TestFixture(typeof(TestInMemoryFileSystem))]
    [TestFixture(typeof(TestLocalFileSystem))]
    public class search_for_absolute_paths<T> : context<T> where T : IFileSystem, new()
    {
        [TestCase(@"c:\test\", @"c:\*")]
        [TestCase(@"c:\test\", @"c:\test\")]
        [TestCase(@"c:\test\", @"c:\test")]
        [TestCase(@"c:\", @"c:\")]
        public void finds_directory(string directoryPath, string searchString)
        {
            var fs = new InMemoryFileSystem();
            fs.GetDirectory(directoryPath).MustExist();

            fs.GetCurrentDirectory().Directories(searchString, SearchScope.CurrentOnly)
                .ShouldHaveCountOf(1)
                .First().Path.FullPath.ShouldBe(directoryPath);
        }
    }
}
