using ICSharpCode.SharpZipLib.Zip;
using LateCat.Installer.Models;
using System;
using System.IO;
using System.Text;

namespace LateCat.Installer.Services
{
    public static class FileOperator
    {
        public static void ExtractorAll(ResourceMap[] resources, IProgress<double> progress)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            ZipStrings.CodePage = Encoding.GetEncoding(936).CodePage;

            var entryConut = 0L;
            foreach (var resource in resources)
            {
                using var tempStream = new MemoryStream();
                resource.ResourceStream.CopyTo(tempStream);

                using var zip = new ZipFile(tempStream);

                entryConut += zip.Count;

                resource.ResourceStream.Position = 0;
            }

            var percent = 100.0 / entryConut;

            var index = 0;

            foreach (var resource in resources)
            {
                if (!Directory.Exists(resource.DestinationDirectory))
                {
                    Directory.CreateDirectory(resource.DestinationDirectory);
                }

                using var inStream = new ZipInputStream(resource.ResourceStream);

                ZipEntry zipEntry;

                while ((zipEntry = inStream.GetNextEntry()) is not null)
                {
                    var entryPath = Path.Combine(resource.DestinationDirectory, zipEntry.Name);
                    var entryDir = Path.GetDirectoryName(entryPath);

                    if (!Directory.Exists(entryDir))
                    {
                        Directory.CreateDirectory(entryDir);
                    }

                    var fileName = Path.GetFileName(zipEntry.Name);

                    if (!string.IsNullOrEmpty(fileName))
                    {
                        using var outStream = File.Create(entryPath);

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
    }
}