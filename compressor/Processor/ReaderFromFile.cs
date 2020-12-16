using System;
using System.IO;

using compressor.Processor.Settings;

namespace compressor.Processor
{
    class ReaderFromFile: Reader
    {
        public ReaderFromFile(SettingsProvider settings)
            : base(settings)
        {
        }

        public sealed override byte[] ReadBlock(Stream input)
        {
            try
            {
                var blockBuffer = new byte[Settings.BlockSize];
                var blockActuallyRead = input.Read(blockBuffer, 0, blockBuffer.Length);
                if(blockActuallyRead != 0)
                {
                    var blockBufferActuallyRead = blockBuffer;
                    if(blockActuallyRead < blockBuffer.Length)
                    {
                        blockBufferActuallyRead = new byte[blockActuallyRead];
                        Array.Copy(blockBuffer, 0, blockBufferActuallyRead, 0, blockBufferActuallyRead.Length);
                    }
                    
                    return blockBufferActuallyRead;
                }
                else
                {
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