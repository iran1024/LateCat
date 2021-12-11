using ICSharpCode.SharpZipLib.Zip;
using LateCat.Installer.Models;
using System;
using System.IO;

namespace LateCat.Installer.Services
{
    public static class FileOperator
    {
        public static void ExtractorAll(ResourceMap[] resources, IProgress<double> progress)
        {
            var entryConut = 0L;
            foreach (var resource in resources)
            {
                using var zip = new ZipFile(resource.ResourceStream);

                entryConut += zip.Count;

                resource.ResourceStream.Position = 0;
            }

            var percent = 100.0 / entryConut;

            foreach (var resource in resources)
            {
                InternalExtractor(resource.ResourceStream, resource.DestinationDirectory, progress, percent);
            }
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

        private static void InternalExtractor(Stream sourceArchiveStream, string destinationDirectoryName, IProgress<double> progress, double percent)
        {
            if (!Directory.Exists(destinationDirectoryName))
            {
                Directory.CreateDirectory(destinationDirectoryName);
            }

            using var inStream = new ZipInputStream(sourceArchiveStream);

            ZipEntry zipEntry;
            var index = 0;

            while ((zipEntry = inStream.GetNextEntry()) is not null)
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
                    using var outStream = File.Create(Path.Combine(destinationDirectoryName, zipEntry.Name));

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
                        outStream.Close();
                    }
                }

                progress.Report(++index * percent);
            }
        }
    }
}