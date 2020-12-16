using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

using compressor.Processor.Settings;

namespace compressor.Processor
{
    class WriterToFile: Writer
    {
        public WriterToFile(SettingsProvider settings, Stream streamToWrite)
            : base(settings, streamToWrite)
        {
        }

        public sealed override void WriteBlock(byte[] data)
        {
            try
            {
                StreamToWrite.Write(data, 0, data.Length);
                StreamToWrite.Flush();
            }
            catch(Exception e)
            {
                throw new ApplicationException("Failed to write block to output file", e);
            }
        }
    }
}