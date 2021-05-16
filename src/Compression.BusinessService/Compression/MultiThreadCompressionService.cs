using Compression.CQRS.Commands;
using Compression.Utils.Files;
using Compression.Utils.Task;
using MediatR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Compression.BusinessService.Compression
{
    public class MultiThreadCompressionService : ICompressionService
    {
        // chunks of 1Mb size
        private const int _chunkSize = 1024 * 1024;
        private readonly object _lock = new object();
        private readonly int _maxThreadsSize = Environment.ProcessorCount * 4;
        private static readonly byte[] GZipDefaultHeader = { 0x1f, 0x8b, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x00 };

        private static Semaphore _semaphore;
        private static MultiThreadTaskExecutor _threadPool;
        private static ConcurrentFileDictionary _fileChunks = new ConcurrentFileDictionary();
        private readonly IMediator _mediator;

        public MultiThreadCompressionService(IMediator mediator)
        {
            _mediator = mediator;
            _threadPool = new MultiThreadTaskExecutor(_maxThreadsSize);
            _semaphore = new Semaphore(_maxThreadsSize, _maxThreadsSize);
        }

        public void Compress(string fileNameToCompress, string archiveFileName)
        {
            var archiveFile = new FileInfo(archiveFileName);

            var fileToCompress = new FileInfo(fileNameToCompress);

            ValidateCompressFile(fileToCompress);

            // delete the file if it's already exists
            if (archiveFile.Exists)
            {
                archiveFile.Delete();
            }

            CompressInChunksFile(fileToCompress, archiveFile);
        }

        public void Decompress(string archiveFileName, string decompressedFileName)
        {
            var archiveFile = new FileInfo(archiveFileName);

            var decompressedFile = new FileInfo(decompressedFileName);

            // delete the file if it's already exists
            if (decompressedFile.Exists)
            {
                decompressedFile.Delete();
            }

            DecompressInChunksFile(archiveFile, decompressedFile);
        }

        public void Dispose()
        {
            _threadPool.Dispose();
            _fileChunks.Clear();
        }

        private static void ValidateCompressFile(FileInfo fileToCompress)
        {
            if (!fileToCompress.Exists)
            {
                throw new FileNotFoundException($"File {fileToCompress.FullName} is not found");
            }

            if ((File.GetAttributes(fileToCompress.FullName) & FileAttributes.Hidden) == FileAttributes.Hidden)
            {
                throw new UnauthorizedAccessException($"File {fileToCompress.FullName} is not accesible");
            }

            if (fileToCompress.Length == 0)
            {
                throw new FileLoadException($"File {fileToCompress.FullName} is empty");
            }
        }

        private void CompressInChunksFile(FileInfo source, FileInfo target)
        {
            var fileLength = source.Length;

            var availableBytes = fileLength;

            var chunkIndex = 0;

            var numberOfBlocks = availableBytes % _chunkSize;

            var resetEvent = new AutoResetEvent(false);

            while (availableBytes > 0)
            {
                var readCount = availableBytes < _chunkSize
                    ? (int)availableBytes
                    : _chunkSize;

                AddToQueue(() =>
                {
                    lock (_lock)
                    {
                        // TODO: refactor as deadlock might occur
                        _mediator.Send(new ReadChunkCommand(source, chunkIndex, fileLength - availableBytes, readCount, _fileChunks)).GetAwaiter().GetResult();

                        _mediator.Send(new CompressChunkCommand(chunkIndex, _fileChunks)).GetAwaiter().GetResult();

                        int nextBlock = _mediator.Send(new WriteChunkCommand(target, chunkIndex, _fileChunks)).GetAwaiter().GetResult();

                        if (nextBlock == numberOfBlocks)
                            resetEvent.Set();
                    }
                });

                _threadPool.Start();

                availableBytes -= readCount;
                chunkIndex++;
            }
        }

        private void DecompressInChunksFile(FileInfo source, FileInfo target)
        {
            using (var reader = new BinaryReader(source.Open(FileMode.Open, FileAccess.Read)))
            {
                var gzipHeader = GZipDefaultHeader;

                var fileLength = source.Length;
                var availableBytes = fileLength;
                var chunkIndex = 0;

                var resetEvent = new AutoResetEvent(false);

                while (availableBytes > 0)
                {
                    var gzipBlock = new List<byte>(_chunkSize);

                    // gzip header.
                    if (chunkIndex == 0)
                    {
                        // get first GZip header from the file. All internal gzip blocks have the same one.
                        gzipHeader = reader.ReadBytes(gzipHeader.Length);
                        availableBytes -= gzipHeader.Length;
                    }
                    gzipBlock.AddRange(gzipHeader);

                    // read gzipped data.
                    var gzipHeaderMatchsCount = 0;
                    while (availableBytes > 0)
                    {
                        var curByte = reader.ReadByte();
                        gzipBlock.Add(curByte);
                        availableBytes--;

                        // check a header of the next gzip block.
                        if (curByte == gzipHeader[gzipHeaderMatchsCount])
                        {
                            gzipHeaderMatchsCount++;
                            if (gzipHeaderMatchsCount != gzipHeader.Length)
                                continue;

                            // remove gzip header of the next block from a rear of this one.
                            gzipBlock.RemoveRange(gzipBlock.Count - gzipHeader.Length, gzipHeader.Length);
                            break;
                        }

                        gzipHeaderMatchsCount = 0;
                    }

                    var gzipBlockStartPosition = 0L;
                    var gzipBlockLength = gzipBlock.ToArray().Length;
                    if (chunkIndex > 0)
                    {
                        gzipBlockStartPosition = fileLength - availableBytes - gzipHeader.Length - gzipBlockLength;

                        // last gzip block in a file is reached
                        if (gzipBlockStartPosition + gzipHeader.Length + gzipBlockLength == fileLength)
                            gzipBlockStartPosition += gzipHeader.Length;
                    }

                    AddToQueue(() =>
                    {
                        lock (_lock)
                        {
                             // TODO: refactor as deadlock might occur
                             _mediator.Send(new ReadChunkCommand(source, chunkIndex, gzipBlockStartPosition, gzipBlockLength, _fileChunks)).GetAwaiter().GetResult();

                            _mediator.Send(new DecompressFileCommand(chunkIndex, _fileChunks)).GetAwaiter().GetResult();

                            int nextBlock = _mediator.Send(new WriteChunkCommand(target, chunkIndex, _fileChunks)).GetAwaiter().GetResult();

                            if (nextBlock == gzipBlockLength)
                                resetEvent.Set();
                        }
                    });

                    chunkIndex++;
                }
            }

            _threadPool.Start();
        }

        private void AddToQueue(Action action)
        {
            _semaphore.WaitOne();
            _threadPool.AddTask(() =>
            {
                try
                {
                    action();
                    _semaphore.Release();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });
        }
    }
}
