using Compression.Utils.Files;
using MediatR;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Compression.CQRS.Commands.Handlers
{
    public class ReadChunkCommandHandler : IRequestHandler<ReadChunkCommand, Unit>
    {
        public Task<Unit> Handle(ReadChunkCommand request, CancellationToken cancellationToken)
        {
            using (var reader = new BinaryReader(request.InputFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                reader.BaseStream.Seek(request.StartPosition, SeekOrigin.Begin);
                var bytes = reader.ReadBytes(request.BytesCount);
                ConcurrentFileDictionary.AddOrUpdate(request.ChunkIndex, bytes);
            }

            return Task.FromResult(Unit.Value);
        }
    }
}
