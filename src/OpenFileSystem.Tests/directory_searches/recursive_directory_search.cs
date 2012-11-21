using System;
using System.Linq;
using NUnit.Framework;
using contexts;

namespace directory_searches
{
    [TestFixture("$TEMP$path/folder/", "$TEMP$**/folder")]
    [TestFixture("$TEMP$path/folder/", "$TEMP$**/folder")]
    [TestFixture("$TEMP$path/folder/", "$TEMP$**/**/folder")]
    [TestFixture("$TEMP$path/folder/", "$TEMP$p*/f*")]
    [TestFixture("$TEMP$path/folder/", "$TEMP$path/f*")]
    [TestFixture("$TEMP$path/folder/", "$TEMP$*/folder/")]
    [TestFixture("$TEMP$path/folder/", "**/folder", "$TEMP$")]
    [TestFixture("$TEMP$path/folder/", "path/**/folder", "$TEMP$")]
    [TestFixture("$TEMP$path/folder/", "path/**/**/folder", "$TEMP$")]
    [TestFixture("$TEMP$path/folder/", "p*/f*", "$TEMP$")]
    [TestFixture("$TEMP$path/folder/", "path/f*", "$TEMP$")]
    [TestFixture("$TEMP$path/folder/", "*/f*", "$TEMP$")]
    [TestFixture("$TEMP$path/folder/", "path/**/*", "$TEMP$")]
    public class recursive_directory_search : file_search_context
    {
        // this test smells bad. Just sayin'
        string existingDirectory;

        public recursive_directory_search(string directory, string searchSpec) : this(directory, searchSpec, null)
        {
        }

        public recursive_directory_search(string directory, string searchSpec, string currentDirectory)
        {
            directory = OS.MakeNative(directory);
            searchSpec = OS.MakeNative(searchSpec);

            existingDirectory = directory;

            if (currentDirectory != null) given_currentDirectory(OS.MakeNative(currentDirectory));

            given_directory(directory);

            when_searching_for_directories(searchSpec);
        }

        [Test]
        public void file_is_found()
        {
            Directories.ShouldHaveCountOf(1).First().Path.FullPath.ShouldBe(existingDirectory);
        }
    }

}