using System;
using System.IO;

using compressor.Processor.Settings;

namespace compressor.Processor
{
    class ProcessorCompress: Processor
    {
        public ProcessorCompress(SettingsProvider settings)
            : base(settings, new ReaderFromFile(settings), new ConverterCompress(settings), new WriterToArchive(settings))
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