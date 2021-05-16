using Compression.CQRS.Commands;
using Compression.Utils.Files;
using Compression.Utils.Task;
using MediatR;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace Compression.BusinessService.Compression
{
    public class MultiThreadCompressionService : ICompressionService
    {
        private const string _compressionExtension = ".gz";

        // chunks of 1Mb size
        private const int _chunkSize = 1024 * 1024;

        private readonly object _lock = new object();

        private readonly int _maxThreadsSize = Environment.ProcessorCount * 4;

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

            ValidateFile(fileToCompress);

            // delete the file if it's already exists
            if (archiveFile.Exists)
            {
                archiveFile.Delete();
            }

            CompressInChunksFile(fileToCompress, archiveFile);
        }
        public void Decompress(string archiveFileName, string decompressedFileName)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _threadPool.Dispose();
            _fileChunks.Clear();
        }

        private static void ValidateFile(FileInfo fileToCompress)
        {
            if (!fileToCompress.Exists)
            {
                throw new FileNotFoundException($"File {fileToCompress.FullName} is not found");
            }

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

                availableBytes -= readCount;
                chunkIndex++;
            }

            //resetEvent.WaitOne();
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
            _threadPool.Start();
        }
    }
}
