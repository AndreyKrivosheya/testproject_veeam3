using System;
using System.IO;

using compressor.Processor.Settings;

namespace compressor.Processor
{
    class ReaderFromArchive : Reader
    {
        public ReaderFromArchive(SettingsProvider settings)
            : base(settings)
        {
        }
        
        public sealed override byte[] ReadBlock(Stream input)
        {
            var blockLengthBuffer = new byte[sizeof(Int64)];
            var blockLengthActuallyRead = input.Read(blockLengthBuffer, 0, blockLengthBuffer.Length);
            if(blockLengthActuallyRead != 0)
            {
                if(blockLengthActuallyRead != sizeof(Int64))
                {
                    throw new InvalidDataException("Failed to read next block size, read less bytes then block size length occupies");
                }
                else
                {
                    var blockBuffer = new byte[GZipStreamHelper.Header.Length + BitConverter.ToInt64(blockLengthBuffer, 0)];
                    var blockActuallyRead = input.Read(blockBuffer, GZipStreamHelper.Header.Length, blockBuffer.Length - GZipStreamHelper.Header.Length);
                    if(blockActuallyRead < blockBuffer.Length - GZipStreamHelper.Header.Length)
                    {
                        throw new InvalidDataException("Failed to read next block, read less bytes then block occupies");
                    }
                    else
                    {
                        // prepend GZipStream header
                        Array.Copy(GZipStreamHelper.Header, 0, blockBuffer, 0, GZipStreamHelper.Header.Length);
            
                        return blockBuffer;
                    }
                }
            }
            else
            {
                return null;
            }
        }
    };
}