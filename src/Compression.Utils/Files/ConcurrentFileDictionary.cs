using System;
using System.Collections.Concurrent;

namespace Compression.Utils.Files
{
    public static class ConcurrentFileDictionary
    {
        private static readonly ConcurrentDictionary<int, byte[]> _chunks = new ConcurrentDictionary<int, byte[]>();

        public static void AddOrUpdate(int index, byte[] bytes)
        {
            _chunks[index] = bytes;
        }

        public static byte[] Get(int index)
        {
            if (!_chunks.ContainsKey(index))
                throw new ArgumentException($"File chunk with index {index} dos not exist.");

            if (_chunks.TryRemove(index, out var chunk))
            {
                return chunk;
            }
            else
            {
                throw new ArgumentException($"Unable to remove {index} from file chunks");
            }
        }

        public static void Clear()
        {
            _chunks.Clear();
        }
    }
}
