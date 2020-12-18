using System;
using System.IO;

using compressor.Processor.Settings;

namespace compressor.Processor
{
    class WriterToArchive : Writer
    {
        public WriterToArchive(SettingsProvider settings, WritingStrategy writingStrategy = null)
            : base(settings, writingStrategy)
        {
        }
        
        public sealed override void WriteBlock(Stream output, byte[] data)
        {
            if(data.Length > GZipStreamHelper.Header.Length)
            {
                // write block data length
                try
                {
                    // strip out GZipStream header
                    WritingStrategy.WriteBytes(output, BitConverter.GetBytes((Int64)(data.Length - GZipStreamHelper.Header.Length)));
                }
                catch(Exception e)
                {
                    throw new ApplicationException("Failed to write block length to archive", e);
                }
                // write block data
                try
                {
                    // strip out GZipStream header
                    WritingStrategy.WriteBytes(output, data, GZipStreamHelper.Header.Length);
                    WritingStrategy.Flush(output);
                }
                catch(Exception e)
                {
                    throw new ApplicationException("Failed to write block to archive", e);
                }
            }
        }
    };
}