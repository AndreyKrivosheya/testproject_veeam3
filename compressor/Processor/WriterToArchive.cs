using System;
using System.IO;

using compressor.Processor.Settings;

namespace compressor.Processor
{
    class WriterToArchive: Writer
    {
        public WriterToArchive(SettingsProvider settings)
            : base(settings)
        {
        }

        public sealed override void WriteBlock(Stream output, byte[] data)
        {
            // write block data length
            try
            {
                var blockLengthBuffer = BitConverter.GetBytes((Int64)data.Length);
                output.Write(blockLengthBuffer, 0, blockLengthBuffer.Length);
            }
            catch(Exception e)
            {
                throw new ApplicationException("Failed to write block length to archive", e);
            }
            // write block data
            try
            {
                output.Write(data, 0, data.Length);
                output.Flush();
            }
            catch(Exception e)
            {
                throw new ApplicationException("Failed to write block to archive", e);
            }
        }
    }
}