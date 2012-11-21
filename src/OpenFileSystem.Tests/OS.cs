using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SysPath = System.IO.Path;

static class OS
{
    public static string MakeNative(string path)
    {
        var withLocations = path.Replace("$TEMP$", SysPath.GetTempPath());

        return string.Join(SysPath.DirectorySeparatorChar + "", withLocations.Split(new[] { '/' }));
    }
}
