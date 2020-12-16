using System;
using System.IO;
using System.Threading;

using compressor.Processor.Settings;

namespace compressor.Processor
{
    class ReaderFromFile: Reader
    {
        public ReaderFromFile(SettingsProvider settings, Stream streamToRead)
            : base(settings, streamToRead)
        {
        }

        int i = 0;
        public sealed override byte[] ReadBlock()
        {
            try
            {
                var blockBuffer = new byte[Settings.BlockSize];
                var blockActuallyRead = StreamToRead.Read(blockBuffer, 0, blockBuffer.Length);
                if(blockActuallyRead != 0)
                {
                    var blockBufferActuallyRead = blockBuffer;
                    if(blockActuallyRead < blockBuffer.Length)
                    {
                        blockBufferActuallyRead = new byte[blockActuallyRead];
                        Array.Copy(blockBuffer, 0, blockBufferActuallyRead, 0, blockBufferActuallyRead.Length);
                    }
                    
                    i++;
                    return blockBufferActuallyRead;
                }
                else
                {
                    i++;
                    return null;
                }
            }
            catch(Exception e)
            {
                throw new ApplicationException("Failed to read block from input file", e);
            }
        }
    }
}