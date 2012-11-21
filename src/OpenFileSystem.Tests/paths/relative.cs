using System;
using NUnit.Framework;
using OpenFileSystem.IO;

namespace paths
{
    public class relative : context
    {
        [Test]
        public void relative_path_is_not_rooted ()
        {
            new Path (OS.MakeNative("test/folder")).IsRooted.ShouldBeFalse ();
        }

        [Test]
        public void relative_path_cannot_be_made_relative()
        {
            Executing(() =>
                      new Path("folder").MakeRelative(new Path(OS.MakeNative("$TEMP$")))
            ).ShouldThrow<InvalidOperationException>();
        }
    }
}