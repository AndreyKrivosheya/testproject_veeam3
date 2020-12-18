using System;
using System.IO;

using compressor.Processor.Settings;

namespace compressor.Processor
{
    abstract class Reader : Component
    {
        public delegate Reader Factory(SettingsProvider settings);
        public static Reader FromFile(SettingsProvider settings)
        {
            return new ReaderFromFile(settings);
        }
        public static Reader FromArchive(SettingsProvider settings)
        {
            return new ReaderFromArchive(settings);
        }

        public Reader(SettingsProvider settings)
            : base(settings)
        {
        }
        
        public virtual byte[] ReadBlock(Stream input)
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
    };
}