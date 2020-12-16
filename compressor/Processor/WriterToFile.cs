using System;
using System.IO;

using compressor.Processor.Settings;

namespace compressor.Processor
{
    class WriterToFile: Writer
    {
        public WriterToFile(SettingsProvider settings)
            : base(settings)
        {
        }

        public sealed override void WriteBlock(Stream ouput, byte[] data)
        {
            try
            {
                ouput.Write(data, 0, data.Length);
                ouput.Flush();
            }
            catch(Exception e)
            {
                throw new ApplicationException("Failed to write block to output file", e);
            }
        }
    }
}