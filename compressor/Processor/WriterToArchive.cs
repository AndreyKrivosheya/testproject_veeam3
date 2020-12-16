using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

using compressor.Processor.Settings;

namespace compressor.Processor
{
    class WriterToArchive: Writer
    {
        public WriterToArchive(SettingsProvider settings, Stream streamToWrite)
            : base(settings, streamToWrite)
        {
        }

        public sealed override void WriteBlock(byte[] data)
        {
            // write block data length
            try
            {
                var blockLengthBuffer = BitConverter.GetBytes((Int64)data.Length);
                StreamToWrite.Write(blockLengthBuffer, 0, blockLengthBuffer.Length);
            }
            catch(Exception e)
            {
                throw new ApplicationException("Failed to write block length to archive", e);
            }
            // write block data
            try
            {
                StreamToWrite.Write(data, 0, data.Length);
                StreamToWrite.Flush();
            }
            catch(Exception e)
            {
                throw new ApplicationException("Failed to write block to archive", e);
            }
        }
    }
}