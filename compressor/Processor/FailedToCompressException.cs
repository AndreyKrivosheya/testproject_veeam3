using System;
using System.IO;

using compressor.Processor.Settings;

namespace compressor.Processor
{
    class FailedToCompressException: ApplicationException
    {
        public FailedToCompressException(Exception e)
            : base("Failed to compress", e)
        {
        }
        public FailedToCompressException(string input, string output, Exception e)
            : base(string.Format("Failed to compress '{0}' to '{1}'", input, output), e)
        {
        }
    };
}