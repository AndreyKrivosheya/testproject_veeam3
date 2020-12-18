using System;
using System.IO;

using compressor.Processor.Settings;

namespace compressor.Processor
{
    class ReadingStrategyFromFileSystem : ReadingStrategy
    {
        public ReadingStrategyFromFileSystem(SettingsProvider settings)
            : base(settings)
        {
        }
        
        public override byte[] ReadBytes(Stream input, long count, bool exactly = false)
        {
            var blockBuffer = new byte[count];
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
    };
}