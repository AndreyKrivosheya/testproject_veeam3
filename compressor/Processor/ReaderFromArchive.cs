using System;
using System.IO;

using compressor.Processor.Settings;

namespace compressor.Processor
{
    class ReaderFromArchive: Reader
    {
        public ReaderFromArchive(SettingsProvider settings)
            : base(settings)
        {
        }

        public sealed override byte[] ReadBlock(Stream input)
        {
            try
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
                        var blockBuffer = new byte[BitConverter.ToInt64(blockLengthBuffer, 0)];
                        var blockActuallyRead = input.Read(blockBuffer, 0, blockBuffer.Length);
                        if(blockActuallyRead < blockBuffer.Length)
                        {
                            throw new InvalidDataException("Failed to read next block, read less bytes then block occupies");
                        }
                        else
                        {
                            return blockBuffer;
                        }
                    }
                }
                else
                {
                    return null;
                }
            }
            catch(Exception e)
            {
                throw new ApplicationException("Failed to read block from archive", e);
            }
        }
    }
}