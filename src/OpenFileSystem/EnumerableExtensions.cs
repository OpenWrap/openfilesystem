using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenFileSystem.IO
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> Copy<T>(this IEnumerable<T> source)
        {
            return new List<T>(source);
        }
    }
}
