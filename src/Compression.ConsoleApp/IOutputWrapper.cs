using System.Text;
using System.Threading.Tasks;

namespace Compression.ConsoleApp
{
    interface IOutputWrapper
    {
        public void Write(char value);

        public void WriteLine(string value);

        public Task WriteLineAsync(string value);

        public Encoding Encoding { get; }

        public string Contents { get; }
    }
}
