using System;
using System.Collections.Concurrent;

namespace Compression.Utils.Files
{
    public class ConcurrentFileDictionary
    {
        private static readonly ConcurrentDictionary<int, byte[]> _chunks = new ConcurrentDictionary<int, byte[]>();

        public void AddOrUpdate(int index, byte[] bytes)
        {
            _chunks[index] = bytes;
        }

        public byte[] Get(int index)
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

        public void Clear()
        {
            _chunks.Clear();
        }
    }
}
