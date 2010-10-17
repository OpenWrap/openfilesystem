﻿using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenFileSystem.IO
{
    public static class FileExtensions
    {
        public static T MustExist<T>(this T directory) where T:IFileSystemItem<T>
        {
            if (!directory.Exists)
                directory.Create();
            return directory;
        }

        public static IDirectory FindDirectory(this IDirectory directory, string path)
        {
            var child = directory.GetDirectory(path);
            return child.Exists ? child : null;
        }

        public static IFile FindFile(this IDirectory directory, string filename)
        {
            var child = directory.GetFile(filename);
            return child.Exists ? child : null;
        }

        public static IDirectory GetOrCreateDirectory(this IDirectory directory, params string[] childDirectories)
        {
            return childDirectories.Aggregate(directory,
                                              (current, childDirectoryName) => MustExist(current.GetDirectory(childDirectoryName)));
        }

        public static Stream OpenRead(this IFile file)
        {
            return file.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
        }
        public static Stream OpenAppend(this IFile file)
        {
            return file.Open(FileMode.Append, FileAccess.Write, FileShare.Read);
        }
        public static Stream OpenWrite(this IFile file)
        {
            return file.Open(FileMode.Create, FileAccess.Write, FileShare.Read);
        }

        public static IEnumerable<IDirectory> AncestorsAndSelf(this IDirectory directory)
        {
            do
            {
                yield return directory;
                directory = directory.Parent;
            } while (directory != null);
        }
    }
}