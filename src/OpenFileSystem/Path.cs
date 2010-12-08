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

            Segments = GenerateSegments(fullPath);
            _normalizedPath = NormalizePath(fullPath);
        }

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
            return string.Join("" + System.IO.Path.DirectorySeparatorChar, GenerateSegments(fullPath).ToArray());
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
    }
}