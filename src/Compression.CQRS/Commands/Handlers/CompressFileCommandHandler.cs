using Compression.Utils.Compression;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Compression.CQRS.Commands.Handlers
{
    public class CompressFileCommandHandler : IRequestHandler<CompressFileCommand, Unit>
    {
        private readonly ICompressionService _compressionService;

        public CompressFileCommandHandler(ICompressionService compressionService)
        {
            _compressionService = compressionService;
        }

        public Task<Unit> Handle(CompressFileCommand request, CancellationToken cancellationToken)
        {
            _compressionService.Compress(request.InputFileName, request.OutputFileName);

            return Task.FromResult(Unit.Value);
        }
    }
}
