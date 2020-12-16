using System;
using System.IO;

using compressor.Processor.Settings;

namespace compressor.Processor
{
    class ProcessorCompress: Processor
    {
        public ProcessorCompress(SettingsProvider settings)
            : base(settings)
        {
        }
         
        protected sealed override Reader CreateReader(Stream input)
        {
            return new ReaderFromFile(Settings, input);
        }
        protected sealed override Converter CreateConverter()
        {
            return new ConverterCompress(Settings);
        }
        protected sealed override Writer CreateWriter(Stream output)
        {
            return new WriterToArchive(Settings, output);
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