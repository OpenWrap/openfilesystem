using System;
using System.Linq;
using NUnit.Framework;
using OpenFileSystem.IO;
using contexts;

namespace directory_searches
{
    [TestFixture(typeof(TestInMemoryFileSystem))]
    [TestFixture(typeof(TestLocalFileSystem))]
    public class search_for_absolute_paths<T> : files<T> where T : IFileSystem, new()
    {
        const string TEMPFOLDER = "CBB7E871-89FF-4F20-A58E-73EB4D2F1191";
        [TestCase(@"c:\" + TEMPFOLDER + @"\test\", @"c:\" + TEMPFOLDER + @"\*")]
        [TestCase(@"c:\" + TEMPFOLDER + @"\test\", @"c:\" + TEMPFOLDER + @"\test\")]
        [TestCase(@"c:\" + TEMPFOLDER + @"\test\", @"c:\" + TEMPFOLDER + @"\test")]
        [TestCase(@"c:\" + TEMPFOLDER + "\\", @"c:\" + TEMPFOLDER)]
        public void finds_directory(string directoryPath, string searchString)
        {
            IDirectory folder = null;
            try
            {
                folder = FileSystem.GetDirectory(directoryPath).MustExist();

                FileSystem.GetCurrentDirectory().Directories(searchString, SearchScope.CurrentOnly)
                    .ShouldHaveCountOf(1)
                    .First().Path.FullPath.ShouldBe(directoryPath);
            }
            finally
            {
                folder.Delete();
            }
        }
    }
}
