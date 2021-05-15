using AutoMapper;
using Compression.ConsoleApp.Dtos;
using Compression.CQRS;
using Compression.CQRS.Commands;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Compression.ConsoleApp
{
    public static class Program
    {
        private const string _commandUsageText = "Usage:\n\tfor compressing the file: compress [original file name] [archive file name]" +
                                                "\n\tfor decompressing the file: decompress [archive file name] [decompressing file name]";

        public static async Task<int> Main()
        {
            var writer = new WrappingWriter(Console.Out);
            var mediator = BuildMediator(writer);

            var mapper = InitiateAutoMapper();

            /*var command = ReadCommand(writer);

            if (command == null) return 1;

            var mediatorCommand = MapInputToCommand(writer, mapper, command);

            if (mediatorCommand == null) return 1;
            */
            try
            {
                var timer = Stopwatch.StartNew();
                using (var compress = new CompressionService())
                {
                    compress.Compress("bigcsv.csv", "tessst");
                    //compress.StartStream(command.InputFileName, command.OutputFileName);
                }
                timer.Stop();
                writer.WriteLine($"File was processed in = {timer.Elapsed.TotalSeconds} s");
                //new ThreadingPlayground();
                //await mediator.Send(mediatorCommand);            
            }
            catch (Exception e)
            {
                writer.WriteLine(e.Message);
            }

            //writer.WriteLine($"Command is {command.Command} {command.InputFileName} {command.OutputFileName}");

            return 0;
        }

        private static object MapInputToCommand(IOutputWrapper writer, IMapper mapper, InputCommand command)
        {
            // TODO: change to dynamic one with dictionary
            switch (command.Command)
            {
                case "compress":
                    return mapper.Map<CompressFileCommand>(command);
                case "decompress":
                    return mapper.Map<DecompressFileCommand>(command);
                default:
                    writer.WriteLine("Usupported command: " + command.Command);
                    return null;
            }
        }

        private static InputCommand ReadCommand(IOutputWrapper writer)
        {
            var inputString = Console.ReadLine();
            try
            {
                return new InputCommand(inputString);
            }
            catch (IOException e)
            {
                writer.WriteLine(e.Message);
                writer.WriteLine(_commandUsageText);
                return null;
            }
        }

        private static IMediator BuildMediator(WrappingWriter writer)
        {
            var services = new ServiceCollection();

            services.AddSingleton<TextWriter>(writer);

            services.AddMediatR(typeof(CompressFileCommand));

            services.AddScoped(typeof(ICompressionService), typeof(CompressionService));

/*            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(GenericPipelineBehavior<,>));
            services.AddScoped(typeof(IRequestPreProcessor<>), typeof(GenericRequestPreProcessor<>));
            services.AddScoped(typeof(IRequestPostProcessor<,>), typeof(GenericRequestPostProcessor<,>));
*/
            var provider = services.BuildServiceProvider();

            return provider.GetRequiredService<IMediator>();
        }

        private static IMapper InitiateAutoMapper()
        {
            var configuration = new MapperConfiguration(cfg => cfg.AddProfile(new AutoMapperConfig()));

            return configuration.CreateMapper();
        }
    }
}
