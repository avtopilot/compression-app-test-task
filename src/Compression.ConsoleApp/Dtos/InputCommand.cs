using System;
using System.IO;

namespace Compression.ConsoleApp.Dtos
{
    public class InputCommand
    {
        public InputCommand(string inputString)
        {
            var splitString = inputString.Split(" ", StringSplitOptions.RemoveEmptyEntries);

            if (splitString.Length != 3) throw new IOException();

            Command = splitString[0];
            InputFileName = splitString[1];
            OutputFileName = splitString[2];
        }

        public string Command { get; set; }
        public string InputFileName { get; set; }
        public string OutputFileName { get; set; }
    }
}
