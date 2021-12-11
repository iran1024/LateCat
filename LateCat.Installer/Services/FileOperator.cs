using ICSharpCode.SharpZipLib.Zip;
using System;
using System.IO;

namespace LateCat.Installer.Services
{
    public static class FileOperator
    {
        public static void Extractor(Stream sourceArchiveStream, string destinationDirectoryName, Progress<int> progress)
        {
            if (!Directory.Exists(destinationDirectoryName))
            {
                Directory.CreateDirectory(destinationDirectoryName);
            }

            InternalExtractor(sourceArchiveStream, destinationDirectoryName);
        }

        public static void Save(Stream stream, string path)
        {
            using var inStream = new BufferedStream(stream);
            using var outStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read);

            try
            {
                var buffer = new byte[1024];
                var length = 0;

                while ((length = inStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    outStream.Write(buffer, 0, length);
                }

                outStream.Flush();
            }
            finally
            {
                if (outStream is not null)
                {
                    outStream.Close();
                }
                if (inStream is not null)
                {
                    inStream.Close();
                }
            }
        }

        private static void InternalExtractor(Stream sourceArchiveStream, string destinationDirectoryName)
        {
            var zip = new ZipFile(sourceArchiveStream);
            var entryCount = zip.Count;

            var percent = entryCount / 100;

            using var zipInStream = new ZipInputStream(sourceArchiveStream);

            ZipEntry zipEntry = null;

            while ((zipEntry = zipInStream.GetNextEntry()) is not null)
            {
                if (zipEntry.IsDirectory)
                {
                    var path = Path.Combine(destinationDirectoryName, zipEntry.Name);
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                }

                var fileName = Path.GetFileName(zipEntry.Name);

                if (!string.IsNullOrEmpty(fileName))
                {
                    using var zipOutStream = File.Create(Path.Combine(destinationDirectoryName, zipEntry.Name));

                    try
                    {
                        var buffer = new byte[1024];
                        var length = 0;

                        while ((length = zipInStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            zipOutStream.Write(buffer, 0, length);
                        }

                        zipOutStream.Flush();
                    }
                    finally
                    {
                        zipOutStream.Close();
                    }
                }
            }
        }
    }
}