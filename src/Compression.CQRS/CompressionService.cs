using System;
using System.IO;
using System.IO.Compression;

namespace Compression.CQRS
{
    public class CompressionService : ICompressionService
    {
        private const string _compressionExtension = ".gz";

        private const int _blockSize = 1222;

        public void Compress(string fileNameToCompress, string archiveFileName)
        {
            if (!File.Exists(fileNameToCompress))
            {
                throw new FileNotFoundException($"File {fileNameToCompress} is not found");
            }

            var archiveFile = new FileInfo(archiveFileName);

            var fileToCompress = new FileInfo(fileNameToCompress);

            if ((File.GetAttributes(fileToCompress.FullName) & FileAttributes.Hidden) == FileAttributes.Hidden)
            {
                throw new UnauthorizedAccessException($"File {fileToCompress.FullName} is not accesible");
            }

            if (fileToCompress.Extension != _compressionExtension)
            {
                throw new FileNotFoundException($"File {fileToCompress.FullName} is already compressed");
            }

            // delete the file if it's already exists
            if (archiveFile.Exists)
            {
                archiveFile.Delete();
            }

            using (FileStream originalFileStream = fileToCompress.OpenRead())
            {
                using (FileStream compressedFileStream = File.Create(archiveFile.FullName + _compressionExtension))
                {
                    using (GZipStream compressionStream = new GZipStream(compressedFileStream, CompressionMode.Compress))
                    {
                        originalFileStream.CopyTo(compressionStream);
                    }
                }
                FileInfo info = new FileInfo(archiveFile.FullName + _compressionExtension);
                Console.WriteLine($"Compressed {fileToCompress.Name} from {fileToCompress.Length} to {info.Length} bytes.");

            }

            //File.Move(fileToCompress.FullName + _compressionExtension, archiveFile.FullName + _compressionExtension);
        }

        public void Decompress(string archiveFileName, string decompressedFileName)
        {
            throw new NotImplementedException();
        }
    }
}
