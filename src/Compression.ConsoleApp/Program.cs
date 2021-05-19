using Compression.BusinessService.Compression;
using Compression.ConsoleApp.Dtos;
using Compression.CQRS.Commands;
using Compression.Utils.Compression;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.IO;

namespace Compression.ConsoleApp
{
    public static class Program
    {
        private const string _commandUsageText = "Usage:\n\tfor compressing the file: compress [original file name] [archive file name]" +
                                                "\n\tfor decompressing the file: decompress [archive file name] [decompressing file name]";

        public static int Main(string[] args)
        {
            var writer = new WrappingWriter(Console.Out);
            var mediator = BuildMediator(writer);

            var command = ReadCommand(args, writer);

            if (command == null) return 1;

            int result;
            try
            {
                var timer = Stopwatch.StartNew();

                result = RunCommand(writer, mediator, command);

                timer.Stop();
                writer.WriteLine($"File was {command.Command.ToLower()}ed in = {timer.Elapsed.TotalSeconds} s"); 
            }
            catch (Exception e)
            {
                writer.WriteLine(e.Message);
                return 0;
            }

            return result;
        }

        private static int RunCommand(IOutputWrapper writer, IMediator mediator, InputCommand command)
        {
            // TODO: change to dynamic one with dictionary
            switch (command.Command.ToLower())
            {
                case "compress":
                    using (var compress = new MultiThreadCompressionService(mediator))
                    {
                        compress.Compress(command.InputFileName, command.OutputFileName);
                    }
                    break;
                case "decompress":
                    using (var compress = new MultiThreadCompressionService(mediator))
                    {
                        compress.Decompress(command.InputFileName, command.OutputFileName);
                    }
                    break;
                default:
                    writer.WriteLine("Usupported command: " + command.Command);
                    return 1;
            }
            return 0;
        }
        
        private static InputCommand ReadCommand(string[] args, IOutputWrapper writer)
        {
            try
            {
                return new InputCommand(args);
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

            services.AddMediatR(typeof(ReadChunkCommand));
            services.AddMediatR(typeof(CompressChunkCommand));
            services.AddMediatR(typeof(WriteChunkCommand));

            services.AddScoped(typeof(ICompressor), typeof(GZipCompressor));

/*            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(GenericPipelineBehavior<,>));
            services.AddScoped(typeof(IRequestPreProcessor<>), typeof(GenericRequestPreProcessor<>));
            services.AddScoped(typeof(IRequestPostProcessor<,>), typeof(GenericRequestPostProcessor<,>));
*/
            var provider = services.BuildServiceProvider();

            return provider.GetRequiredService<IMediator>();
        }
    }
}
