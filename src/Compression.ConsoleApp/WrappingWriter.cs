using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Compression.ConsoleApp
{
    public class WrappingWriter : TextWriter, IOutputWrapper
    {
        private readonly TextWriter _innerWriter;
        private readonly StringBuilder _stringWriter = new StringBuilder();

        public WrappingWriter(TextWriter innerWriter)
        {
            _innerWriter = innerWriter;
        }

        public override void Write(char value)
        {
            _stringWriter.Append(value);
            _innerWriter.Write(value);
        }

        public override void WriteLine(string value)
        {
            _stringWriter.AppendLine(value);
            _innerWriter.WriteLine(value);
        }

        public override Task WriteLineAsync(string value)
        {
            _stringWriter.AppendLine(value);
            return _innerWriter.WriteLineAsync(value);
        }

        public override Encoding Encoding => _innerWriter.Encoding;

        public string Contents => _stringWriter.ToString();
    }
}
