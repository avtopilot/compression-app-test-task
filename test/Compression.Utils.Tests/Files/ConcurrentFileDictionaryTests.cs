using Compression.Utils.Files;
using System;
using System.Text;
using Xunit;

namespace Compression.Utils.Tests.Files
{
    public class ConcurrentFileDictionaryTests : IDisposable
    {
        private ConcurrentFileDictionary _dictionary;

        public ConcurrentFileDictionaryTests()
        {
            _dictionary = new ConcurrentFileDictionary();
        }

        public void Dispose()
        {
            _dictionary.Clear();
        }

        [Theory]
        [InlineData(0, "Hello World")]
        [InlineData(1, "Hello World")]
        public void ConcurrentFileDictionary_Should_Add_Chunks(int index, string input)
        {
            //Arrange
            var bytes = Encoding.UTF8.GetBytes(input);

            //Act
            _dictionary.AddOrUpdate(index, bytes);

            //Assert
            Assert.Equal(1, _dictionary.Length);
        }

        [Fact]
        public void ConcurrentFileDictionary_Should_Remove_Chunks()
        {
            //Arrange
            var bytes = Encoding.UTF8.GetBytes("Hello world");
            var bytes2 = Encoding.UTF8.GetBytes("Hello world! It's me");

            //Act
            _dictionary.AddOrUpdate(0, bytes);
            _dictionary.AddOrUpdate(1, bytes2);
            _dictionary.Clear();

            //Assert
            Assert.Equal(0, _dictionary.Length);
        }

        [Theory]
        [InlineData(0, "Hello World")]
        [InlineData(1, "Hello World")]
        public void ConcurrentFileDictionary_Should_Get_Chunks(int index, string input)
        {
            //Arrange
            var bytes = Encoding.UTF8.GetBytes(input);

            //Act
            _dictionary.AddOrUpdate(index, bytes);

            //Assert
            Assert.Equal(bytes, _dictionary.Get(index));
        }

        [Theory]
        [InlineData(0, "Hello World", "Test 2")]
        [InlineData(1, "Test 2", "Hello World")]
        public void ConcurrentFileDictionary_Should_Update_Chunks(int index, string originalInput, string finalInput)
        {
            //Arrange
            var originalBytes = Encoding.UTF8.GetBytes(originalInput);
            var finalBytes = Encoding.UTF8.GetBytes(finalInput);

            //Act
            _dictionary.AddOrUpdate(index, originalBytes);
            _dictionary.AddOrUpdate(index, finalBytes);

            //Assert
            Assert.Equal(finalBytes, _dictionary.Get(index));
        }

        [Fact]
        public void ConcurrentFileDictionary_Should_Throw_Exception_For_Invalid_Index()
        {
            //Arrange
            var originalBytes = Encoding.UTF8.GetBytes("Hello World");

            //Act
            _dictionary.AddOrUpdate(1, originalBytes);

            //Assert
            Assert.Throws<ArgumentException>(() => _dictionary.Get(0));
        }
    }
}
