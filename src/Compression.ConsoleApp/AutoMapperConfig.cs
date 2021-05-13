using AutoMapper;
using Compression.ConsoleApp.Dtos;
using Compression.CQRS.Commands;

namespace Compression.ConsoleApp
{
    public class AutoMapperConfig : Profile
    {
        public AutoMapperConfig()
        {
            CreateMap<InputCommand, CompressFileCommand>();

            CreateMap<InputCommand, DecompressFileCommand>();
        }
    }
}
