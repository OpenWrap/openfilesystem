using NUnit.Framework;
using OpenFileSystem.IO;

namespace paths
{
    [TestFixture]
    public class unc : context
    {
        [TestCase(@"\\server\path\")]
        [TestCase("//server/path/")]
        public void is_detected(string path)
        {
            new Path(path).IsUnc.ShouldBeTrue();
        }
        
        [TestCase(@"\\server\")]
        [TestCase("//server/")]
        public void things_looking_like_unc_are_not_detected(string path)
        {
            new Path(path).IsUnc.ShouldBeFalse();
        }

        [TestCase(@"\\server\a\file")]
        [TestCase("//server/a/file")]
        public void has_segments(string path)
        {
            new Path(path).Segments.ShouldHaveSameElementsAs(new[] { "a", "file" });
        }

        [TestCase(@"\\server\a\file")]
        [TestCase("//serving/a/file")]
        public void is_rooted(string path)
        {
            new Path(path).IsRooted.ShouldBeTrue();
        }

        [Test]
        public  void roundtrips()
        {
            new Path(@"\\server\path\to\file").ToString().ShouldBe(@"\\server\path\to\file");
        }

        [TestCase(@"\\server\path\to\file",@"\\server\path\to")]
        [TestCase("//server/path/to/file",@"//server/path/to")]
        public void can_make_relative(string path, string root)
        {
            new Path(path).MakeRelative(new Path(root))
                .ToString().ShouldBe("file");
        }

        [TestCase(@"\\server\path")]
        [TestCase("//server/path")]
        public void hostname_is_correct(string path)
        {
            new Path(path).HostName.ShouldBe("server");
        }


    }
}