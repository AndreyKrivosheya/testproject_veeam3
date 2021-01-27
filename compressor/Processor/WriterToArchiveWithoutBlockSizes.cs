using System;
using System.IO;

using compressor.Processor.Settings;

namespace compressor.Processor
{
    class WriterToArchiveWithoutBlockSizes : Writer
    {
        public WriterToArchiveWithoutBlockSizes(SettingsProvider settings, WritingStrategy writingStrategy = null)
            : base(settings, writingStrategy)
        {
        }
       
        protected sealed override void WriteBlock(Stream output, byte[] data, bool flush)
        {
            try
            {
                WritingStrategy.WriteBytes(output, data);
                if(flush)
                {
                    WritingStrategy.Flush(output);
                }
            }
            catch(Exception e)
            {
                throw new ApplicationException("Failed to write block to archive", e);
            }
        }
    };
}