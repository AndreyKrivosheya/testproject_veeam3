using System;
using System.IO;

using compressor.Processor.Settings;

namespace compressor.Processor
{
    class ReaderFromArchive : Reader
    {
        public ReaderFromArchive(SettingsProvider settings, ReadingStrategy readingStrategy = null)
            : base(settings, readingStrategy)
        {
        }
        
        public sealed override byte[] ReadBlock(Stream input)
        {
            try
            {
                var blockLengthBytes = ReadingStrategy.ReadBytes(input, sizeof(Int64), exactly: true);
                if(blockLengthBytes != null)
                {
                    if(blockLengthBytes.Length != sizeof(Int64))
                    {
                        throw new InvalidDataException("Failed to read next block size, read less bytes then block size length occupies");
                    }
                    else
                    {
                        var blockLength = BitConverter.ToInt64(blockLengthBytes, 0);
                        var blockBytes = ReadingStrategy.ReadBytes(input, blockLength, exactly: true);
                        if(blockBytes == null || blockBytes.Length < blockLength)
                        {
                            throw new InvalidDataException("Failed to read next block, read less bytes then block occupies");
                        }
                        else
                        {
                            using(var blockWithHeader = new MemoryStream((int)(GZipStreamHelper.Header.Length + blockLength)))
                            using(var blockWithHeaderWriter = new BinaryWriter(blockWithHeader))
                            {
                                blockWithHeaderWriter.Write(GZipStreamHelper.Header);   // prepend GZipStream header
                                blockWithHeaderWriter.Write(blockBytes);
                
                                return blockWithHeader.ToArray();
                            }
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
    };
}