using Compression.CQRS.Commands;
using Compression.CQRS.Commands.Handlers;
using Compression.Utils.Compression;
using Compression.Utils.Files;
using MediatR;
using Moq;
using System.Text;
using Xunit;

namespace Compression.CQRS.Tests.Commands.Handlers
{
    public class CompressionChunkCommandHandlerTests
    {
        [Fact]
        public async void CompressChunkCommand_Should_Update_Chunk_With_CompressedChunk()
        {
            //Arrange
            var mediator = new Mock<IMediator>();
            var compressor = new Mock<ICompressor>();

            var originalBytes = Encoding.UTF8.GetBytes("Hello World!");
            var compressedBytes = Encoding.UTF8.GetBytes("Hello!");
            compressor.Setup(x => x.Compress(originalBytes)).Returns(compressedBytes);

            var dictionary = new ConcurrentFileDictionary();
            dictionary.AddOrUpdate(0, originalBytes);

            var command = new CompressChunkCommand(0, dictionary);
            var handler = new CompressChunkCommandHanlder(compressor.Object);

            //Act
            Unit x = await handler.Handle(command, new System.Threading.CancellationToken());

            //Assert
            Assert.Equal(compressedBytes, dictionary.Get(0));
        }
    }
}
