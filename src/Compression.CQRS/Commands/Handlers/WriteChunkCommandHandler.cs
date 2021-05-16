using MediatR;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Compression.CQRS.Commands.Handlers
{
    public class WriteChunkCommandHandler : IRequestHandler<WriteChunkCommand, int>
    {
        private static volatile int _nextWriteChunk = 0;

        public Task<int> Handle(WriteChunkCommand request, CancellationToken cancellationToken)
        {
            using (var writer = new BinaryWriter(request.OutputFile.Open(FileMode.OpenOrCreate, FileAccess.Write, FileShare.None)))
            {
                writer.BaseStream.Seek(0, SeekOrigin.End);
                byte[] bytes = request.FileChunks.Get(request.ChunkIndex);
                writer.Write(bytes);
            }

            _nextWriteChunk++;

            return Task.FromResult(_nextWriteChunk);
        }
    }
}
