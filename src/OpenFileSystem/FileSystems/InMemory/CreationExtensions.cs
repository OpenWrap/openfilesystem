using System;

namespace OpenFileSystem.IO.FileSystems.InMemory
{
    public static class CreationExtensions
    {
        public static T CreateChildDir<T>(this T fs, string path, params Action<ChildItem>[] children)
            where T:IFileSystem
        {
            var directory = new ChildItem(fs.GetDirectory(path).MustExist());
            foreach(var item in children)
            {
                item(directory);
            }
            return fs;
        }
    }
}