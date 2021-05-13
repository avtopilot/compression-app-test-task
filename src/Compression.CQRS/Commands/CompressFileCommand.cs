using MediatR;

namespace Compression.CQRS.Commands
{
    public class CompressFileCommand : IRequest
    {
        public string InputFileName { get; set; }
        public string OutputFileName { get; set; }
    }
}
