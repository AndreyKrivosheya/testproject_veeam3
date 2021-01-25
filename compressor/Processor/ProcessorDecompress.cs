using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

using compressor.Processor.Settings;

namespace compressor.Processor
{
    class ProcessorDecompress: Processor
    {
        public ProcessorDecompress(SettingsProvider settings)
            : base(settings, Reader.FromArchiveWithoutBlockSizes, Converter.Decompress, Writer.ToFile)
        {
        }

        public sealed override void Process(Stream input, Stream output)
        {
            try
            {
                base.Process(input, output);
            }
            catch(Exception e)
            {
                throw new FailedToDecompressException(e);
            }
        }

        public sealed override void Process(string input, string output)
        {
            try
            {
                base.Process(input, output);
            }
            catch(FailedToDecompressException e)
            {
                throw new FailedToDecompressException(input, output, e.InnerException);
            }
            catch(Exception e)
            {
                throw new FailedToDecompressException(input, output, e);
            }
        }
    };
}