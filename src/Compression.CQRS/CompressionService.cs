using Compression.Utils.Task;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace Compression.CQRS
{
    public class CompressionService : ICompressionService
    {
        private const string _compressionExtension = ".gz";

        // chunks of 1Mb size
        private const int _chunkSize = 1024 * 1024;

        private readonly object _lock = new object();

        private readonly int _maxThreadsSize = Environment.ProcessorCount * 4;

        private static MultiThreadTaskExecutor _threadPool;

        public CompressionService()
        {
            _threadPool = new MultiThreadTaskExecutor(_maxThreadsSize);
        }

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

            if (fileToCompress.Extension == _compressionExtension)
            {
                throw new FileNotFoundException($"File {fileToCompress.FullName} is already compressed");
            }

            if (fileToCompress.Length == 0)
            {
                throw new FileLoadException($"File {fileToCompress.FullName} is empty");
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
                    CompressInChunks(originalFileStream, compressedFileStream);
                }
            }
        }

        public void Decompress(string archiveFileName, string decompressedFileName)
        {
            throw new NotImplementedException();
        }

        private void CompressInChunks(Stream source, Stream target)
        {
            var blockAmount = (int)(source.Length / _chunkSize + (source.Length % _chunkSize > 0 ? 1 : 0));

            var blockCurrent = 0;

            target.Seek(0, SeekOrigin.Begin);
            source.Seek(0, SeekOrigin.Begin);

            var resetEvent = new AutoResetEvent(false);

            while (source.Position < source.Length)
            {
                var data = new byte[_chunkSize];
                var dataLength = source.Read(data, 0, _chunkSize);
                
                AddToQueue(() =>
                {
                    lock (_lock)
                    {

                        using (var result = new MemoryStream())
                        {
                            using (var compressionStream = new GZipStream(result, CompressionMode.Compress))
                            {
                                compressionStream.Write(data, 0, dataLength);
                            }
                            target.Write(result.ToArray(), 0, result.ToArray().Length);
                        }

                        // release if the last block was processed
                        if (++blockCurrent == blockAmount)
                            resetEvent.Set();

                    }
                });
            }
            
            resetEvent.WaitOne();
        }

        private void AddToQueue(Action action)
        {
            _threadPool.AddTask(() => {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });
            _threadPool.Start();
        }

        public void Dispose()
        {
            _threadPool.Dispose();
        }
    }
}
