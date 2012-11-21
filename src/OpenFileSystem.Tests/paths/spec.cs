using System.Linq;
using NUnit.Framework;
using OpenFileSystem.IO;

namespace paths
{
    public class spec : context
    {
        [Test]
        public void has_segments ()
        {
            var path = new Path (OS.MakeNative("$TEMP$mordor/nurn"));
            var seg = path.Segments.ToArray();
            seg[seg.Length - 2].ShouldBe("mordor");
            seg[seg.Length - 1].ShouldBe("nurn");
        }

        [TestCase(@"$TEMP$test", @"$TEMP$")]
        [TestCase(@"$TEMP$test/", @"$TEMP$test/")]
        public void directory_depends_on_position_of_separator (string path, string expected)
        {
            path = OS.MakeNative(path);
            expected = OS.MakeNative(expected);

            new Path (path).DirectoryName.ShouldBe (expected);
        }

        [Test]
        public void host_is_localhost_by_default()
        {
            new Path(OS.MakeNative("$TEMP$")).HostName.ShouldBe("localhost");
        }


    }
}