using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

using compressor.Processor.Settings;

namespace compressor.Processor
{
    class FailedToDecompressException: ApplicationException
    {
        public FailedToDecompressException(Exception e)
            : base("Failed to decompress", e)
        {
        }
        public FailedToDecompressException(string input, string output, Exception e)
            : base(string.Format("Failed to decompress '{0}' to '{1}'", input, output), e)
        {
        }
    };
}