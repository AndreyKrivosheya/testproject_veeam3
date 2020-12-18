using System;
using System.IO;

using compressor.Processor.Settings;

namespace compressor.Processor
{
    class WriterToFile : Writer
    {
        public WriterToFile(SettingsProvider settings, WritingStrategy writingStrategy = null)
            : base(settings, writingStrategy)
        {
        }
        
        public sealed override void WriteBlock(Stream output, byte[] data)
        {
            try
            {
                WritingStrategy.WriteBytes(output, data);
                WritingStrategy.Flush(output);
            }
            catch(Exception e)
            {
                throw new ApplicationException("Failed to write block to output file", e);
            }
        }
    };
}