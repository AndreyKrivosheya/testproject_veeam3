using System;
using System.IO;

using compressor.Processor.Settings;

namespace compressor.Processor
{
    class ProcessorCompress: Processor
    {
        public ProcessorCompress(SettingsProvider settings)
            : base(settings, Reader.FromFile, Converter.Compress, Writer.ToArchiveWithoutBlockSizes)
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
                throw new FailedToCompressException(e);
            }
        }

        public sealed override void Process(string input, string output)
        {
            try
            {
                base.Process(input, output);
            }
            catch(FailedToCompressException e)
            {
                throw new FailedToCompressException(input, output, e.InnerException);
            }
            catch(Exception e)
            {
                throw new FailedToCompressException(input, output, e);
            }
        }
    };
}