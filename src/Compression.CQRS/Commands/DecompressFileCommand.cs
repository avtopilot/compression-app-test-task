using MediatR;

namespace Compression.CQRS.Commands
{
    public class DecompressFileCommand : IRequest
    {
        public string FileName { get; set; }
    }
}
