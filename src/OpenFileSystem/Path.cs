using System;
using System.Collections.Generic;
using System.Linq;
using SPath = System.IO.Path;

namespace OpenFileSystem.IO
{
    public class Path : IEquatable<Path>
    {
        public Path(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath)) throw new ArgumentNullException("fullPath");
            FullPath = fullPath;
            string hostName;
            IsUnc = IsUncPath(fullPath, out hostName);
            IsRooted = IsUnc || SPath.IsPathRooted(fullPath);
            HostName = hostName;

            Segments = GenerateSegments(fullPath, IsUnc);
        }


        public bool IsUnc { get; private set; }

        public string DirectoryName
        {
            get
            {
                return IsDirectoryPath ? FullPath : EnsureTrailingSlash(System.IO.Path.GetDirectoryName(FullPath));
            }
        }
        static string EnsureTrailingSlash(string path)
        {
            if (path[path.Length - 1] == SPath.DirectorySeparatorChar ||
                path[path.Length - 1] == SPath.AltDirectorySeparatorChar)
                return path;
            return path + SPath.DirectorySeparatorChar;
        }

        public bool IsRooted { get; private set; }

        static IEnumerable<string> GenerateSegments(string path, bool unc)
        {
            var separator = unc ? new[]{ path[0] } : new[] { System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar };
            IEnumerable<string> components = path.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            if (unc) components = components.Skip(1);
            return components.ToList().AsReadOnly();
        }

        public string FullPath { get; private set; }

        public IEnumerable<string> Segments
        {
            get;
            private set;
        }
        public bool IsDirectoryPath { get { return FullPath.EndsWith(System.IO.Path.DirectorySeparatorChar + "") || FullPath.EndsWith(System.IO.Path.AltDirectorySeparatorChar + ""); } }
        public Path Combine(params string[] paths)
        {
            var combinedPath = paths.Aggregate(FullPath, System.IO.Path.Combine);
            return new Path(combinedPath);
        }
        
        public bool Equals(Path other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.FullPath.Equals(FullPath);
        }

        static bool IsUncPath(string fullPath, out string hostName)
        {
            hostName = "localhost";
            char sep = '\0';
            if (fullPath.StartsWith("\\\\") && fullPath.Length > 2 && fullPath[2] != '?')
                sep = '\\';
            else if (fullPath.StartsWith("//"))
                sep = '/';
            if (sep == '\0') return false;

            var serverNameSeparator = fullPath.IndexOf(sep, 2);
            if (serverNameSeparator == -1) return false;

            var validUnc = fullPath.Length > serverNameSeparator+1;
            if (validUnc) hostName = fullPath.Substring(2, serverNameSeparator-2);
            return validUnc;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (!(obj is Path)) return false;
            return Equals((Path)obj);
        }

        public override int GetHashCode()
        {
            return (FullPath != null ? FullPath.GetHashCode() : 0);
        }
        public override string ToString()
        {
            return FullPath;
        }
        public static bool operator ==(Path left, Path right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Path left, Path right)
        {
            return !Equals(left, right);
        }
        
        public static implicit operator string(Path path)
        {
            return path.ToString();
        }

        public string HostName { get; private set; }

        public Path MakeRelative(Path path)
        {
            if (!IsRooted || !path.IsRooted)
                throw new InvalidOperationException("Cannot make relative paths without absolute paths!");
 
            var leftOverSegments = new List<string>();
            var relativeSegmentCount = 0;


            var thisEnum = Segments.GetEnumerator();
            var rootEnum = path.Segments.GetEnumerator();

            bool thisHasValue;
            bool rootHasValue;
            do
            {
                thisHasValue = thisEnum.MoveNext();
                rootHasValue = rootEnum.MoveNext();

                if (thisHasValue && rootHasValue && thisEnum.Current != null && rootEnum.Current != null)
                {
                    if (thisEnum.Current.Equals(rootEnum.Current, StringComparison.OrdinalIgnoreCase))
                        continue;
                }
                if (thisHasValue)
                    leftOverSegments.Add(thisEnum.Current);
                if (rootHasValue)
                    relativeSegmentCount++;
            } while (thisHasValue || rootHasValue);

            var relativeSegment = Enumerable.Repeat("..", relativeSegmentCount).Aggregate("", System.IO.Path.Combine);
            var finalSegment = System.IO.Path.Combine(relativeSegment, leftOverSegments.Aggregate("", System.IO.Path.Combine));
            return new Path(finalSegment);
        }
    }
}