namespace Compression.Utils
{
    public class Constants
    {
        #region GZIP
        /// <summary>
        /// GZip header structure.
        /// http://www.zlib.org/rfc-gzip.html
        /// +---+---+---+---+---+---+---+---+---+---+
        /// |ID1|ID2|CM |FLG|     MTIME     |XFL|OS | (more-->)
        /// +---+---+---+---+---+---+---+---+---+---+
        /// +=======================+
        /// |...compressed blocks...| (more-->)
        /// +=======================+
        /// +---+---+---+---+---+---+---+---+
        /// |     CRC32     |     ISIZE     |
        /// +---+---+---+---+---+---+---+---+
        /// </summary>
        public static readonly byte[] GZipDefaultHeader = { 0x1f, 0x8b, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x00 };
        #endregion
    }
}
