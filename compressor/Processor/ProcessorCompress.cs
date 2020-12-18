using System;
using System.IO;

using compressor.Processor.Settings;

namespace compressor.Processor
{
    class ProcessorCompress: Processor
    {
        public ProcessorCompress(SettingsProvider settings)
            : base(settings, Reader.FromFile, Converter.Compress, Writer.ToArchive)
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
                throw new ApplicationException("Failed to compress", e);
            }
        }
    };
}