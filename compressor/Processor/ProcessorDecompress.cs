using System;
using System.IO;

using compressor.Processor.Settings;

namespace compressor.Processor
{
    class ProcessorDecompress: Processor
    {
        public ProcessorDecompress(SettingsProvider settings)
            : base(settings)
        {
        }
         
        protected sealed override Reader CreateReader(Stream input)
        {
            return new ReaderFromArchive(Settings, input);
        }
        protected sealed override Converter CreateConverter()
        {
            return new ConverterDecompress(Settings);
        }
        protected sealed override Writer CreateWriter(Stream output)
        {
            return new WriterToFile(Settings, output);
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