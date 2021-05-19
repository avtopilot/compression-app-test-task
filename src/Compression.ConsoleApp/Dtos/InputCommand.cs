using System.IO;

namespace Compression.ConsoleApp.Dtos
{
    public class InputCommand
    {
        public InputCommand(string[] input)
        {
            if (input.Length != 3) throw new IOException();

            Command = input[0];
            InputFileName = input[1];
            OutputFileName = input[2];
        }

        public string Command { get; set; }
        public string InputFileName { get; set; }
        public string OutputFileName { get; set; }
    }
}
