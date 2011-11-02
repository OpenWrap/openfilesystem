using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenFileSystem.IO
{
    public class Path : IEquatable<Path>
    {
        readonly string _normalizedPath;

        public Path(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath)) throw new ArgumentNullException("fullPath");
            FullPath = fullPath;
            
            IsRooted = System.IO.Path.IsPathRooted(fullPath);
            IsUnc = IsUncPath(fullPath);

            Segments = GenerateSegments(fullPath);
            _normalizedPath = NormalizePath(fullPath);
        }

        public bool IsUnc { get; private set; }

        public string DirectoryName { get { return IsDirectoryPath ? _normalizedPath : System.IO.Path.GetDirectoryName(FullPath);}}
        public bool IsRooted { get; private set; }

        static IEnumerable<string> GenerateSegments(string path)
        {
            return path.Split(new[] { System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar },StringSplitOptions.RemoveEmptyEntries).ToList().AsReadOnly();
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
            return NormalizePath(other.FullPath).Equals(_normalizedPath, StringComparison.OrdinalIgnoreCase);
        }

        static string NormalizePath(string fullPath)
        {
            var segmentPath = string.Join("" + System.IO.Path.DirectorySeparatorChar, GenerateSegments(fullPath).ToArray());
            return System.IO.Path.IsPathRooted(fullPath) && IsUncPath(fullPath)
                       ? new string(System.IO.Path.DirectorySeparatorChar, 2) + segmentPath
                       : segmentPath;
        }

        static bool IsUncPath(string fullPath)
        {
            return (
                (fullPath.StartsWith("\\\\") && fullPath.Length > 2 && fullPath[2] != '?')
                || fullPath.StartsWith("//"));
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
            return (_normalizedPath != null ? _normalizedPath.GetHashCode() : 0);
        }
        public override string ToString()
        {
            return _normalizedPath;
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
        public Path MakeRelative(Path path)
        {
            if (!IsRooted)
                return this;
            List<string> leftOverSegments = new List<string>();
            int relativeSegmentCount = 0;


            var thisEnum = Segments.GetEnumerator();
            var rootEnum = path.Segments.GetEnumerator();

            bool thisHasValue;
            bool rootHasValue;
            do
            {
                thisHasValue = thisEnum.MoveNext();
                rootHasValue = rootEnum.MoveNext();

                if (thisHasValue && rootHasValue)
                {
                    if (thisEnum.Current.Equals(rootEnum.Current, StringComparison.OrdinalIgnoreCase))
                        continue;
                }
                if (thisHasValue)
                {
                    leftOverSegments.Add(thisEnum.Current);
                }
                if (rootHasValue)
                    relativeSegmentCount++;
            } while (thisHasValue || rootHasValue);

            var relativeSegment = Enumerable.Repeat("..", relativeSegmentCount).Aggregate("", System.IO.Path.Combine);
            var finalSegment = System.IO.Path.Combine(relativeSegment, leftOverSegments.Aggregate("", System.IO.Path.Combine));
            return new Path(finalSegment);
        }
    }
}