using NUnit.Framework;
using OpenFileSystem.IO;

namespace paths
{
    public class absolute : context
    {
        [Test]
        public void absolute_path_is_rooted ()
        {
            new Path (OS.MakeNative("$TEMP$test/folder")).IsRooted.ShouldBeTrue ();
        }

        [TestCase("$TEMP$test/folder", "$TEMP$test", "folder")]
        [TestCase("$TEMP$test/folder", "$TEMP$test/another", "../folder")]
        [TestCase("$TEMP$test/folder", "$TEMP$test/nested/folder", "../../folder")]
        public void absolute_path_is_made_relative (string source, string basePath, string result)
        {
            source = OS.MakeNative(source);
            basePath = OS.MakeNative(basePath);
            result = OS.MakeNative(result);


            new Path (source)
                .MakeRelative (new Path (basePath))
                .FullPath.ShouldBe (result);
        }

    }
}