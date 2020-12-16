using System;
using System.IO;

using compressor.Processor.Settings;

namespace compressor.Processor
{
    class ProcessorDecompress: Processor
    {
        public ProcessorDecompress(SettingsProvider settings)
            : base(settings, new ReaderFromArchive(settings), new ConverterDecompress(settings), new WriterToFile(settings))
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
                throw new ApplicationException("Failed to decompress", e);
            }
        }
    };
}