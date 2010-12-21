using System;
using System.IO;
using OpenFileSystem.IO;
using OpenWrap.Testing;

namespace OpenFileSystem.Tests.open_for_write
{
    public static class StreamExtensions
    {
        public static void Write(this IFile file, Action<Stream> writer, FileMode mode = FileMode.Create, FileAccess access = FileAccess.Write, FileShare share = FileShare.None)
        {
            using (var stream = file.Open(mode, access, share))
                writer(stream);
        }
        public static void Write(this IFile file, byte data = (byte)0, FileMode mode = FileMode.Create, FileAccess access = FileAccess.Write, FileShare share = FileShare.None)
        {
            using (var stream = file.Open(mode, access, share))
                stream.WriteByte(data);
        }
        public static IFile ShouldBe(this IFile file, params byte[] content)
        {
            using (var stream = file.OpenRead())
                stream.ReadToEnd().ShouldBe(content);
            return file;
        }

        public static byte[] ReadToEnd(this Stream stream)
        {
            var streamToReturn = stream as MemoryStream;
            if (streamToReturn == null)
            {
                streamToReturn = new MemoryStream();
                stream.CopyTo(streamToReturn);
                streamToReturn.Position = 0;
            }

            var destinationBytes = new byte[streamToReturn.Length - streamToReturn.Position];
            Buffer.BlockCopy(streamToReturn.GetBuffer(),
                             (int)streamToReturn.Position,
                             destinationBytes,
                             0,
                             (int)(streamToReturn.Length - streamToReturn.Position));
            return destinationBytes;
        }
    }
}