using System;
using System.Linq;
using NUnit.Framework;

namespace file_searches
{
    [TestFixture("$TEMP$path/file.txt", "$TEMP$**/file.txt")]
    [TestFixture("$TEMP$path/file.txt", "$TEMP$path/**/file.txt")]
    [TestFixture("$TEMP$path/file.txt", "$TEMP$path/**/**/file.txt")]
    [TestFixture("$TEMP$path/file.txt", "$TEMP$p*/*.txt")]
    [TestFixture("$TEMP$path/file.txt", "$TEMP$path/*.txt")]
    [TestFixture("$TEMP$path/file.txt", "**/file.txt", "$TEMP$")]
    [TestFixture("$TEMP$path/file.txt", "path/**/file.txt", "$TEMP$")]
    [TestFixture("$TEMP$path/file.txt", "path/**/**/file.txt", "$TEMP$")]
    [TestFixture("$TEMP$path/file.txt", "p*/*.txt", "$TEMP$")]
    [TestFixture("$TEMP$path/file.txt", "path/*.txt", "$TEMP$")]
    [TestFixture("$TEMP$path/file.txt", "$TEMP$path/file.txt")]
    [TestFixture("$TEMP$path/file.txt", "$TEMP$path/file.txt", "$TEMP$path/")]
    public class recursive_file_search : contexts.file_search_context
    {
        readonly string existingFile;

        public recursive_file_search(string file, string searchSpec) : this(file, searchSpec, null)
        {
        }

        public recursive_file_search(string file, string searchSpec, string currentDirectory)
        {
            file = OS.MakeNative(file);
            searchSpec = OS.MakeNative(searchSpec);
            if (currentDirectory != null) currentDirectory = OS.MakeNative(currentDirectory);

            existingFile = file;
            if (currentDirectory != null)
                given_currentDirectory(currentDirectory);
            given_file(file);


            when_searching_for_files(searchSpec);
        }

        [Test]
        public void file_is_found()
        {
            Files.ShouldHaveCountOf(1).First().Path.FullPath.ShouldBe(existingFile);
        }
    }
}