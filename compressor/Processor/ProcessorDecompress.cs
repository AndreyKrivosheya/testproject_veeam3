using System;
using System.IO;
using System.IO.Compression;

using compressor.Processor.Settings;

namespace compressor.Processor
{
    class ProcessorDecompress: Processor
    {
        public ProcessorDecompress(SettingsProvider settings) : base(settings)
        {
        }
         
        protected sealed override byte[] ReadBlock(Stream input)
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

        protected sealed override byte[] ConvertBlock(byte[] data)
        {
            try
            {
                var dataWithHeader = new byte[GZipStreamHelper.Header.Length + data.Length];
                Array.Copy(GZipStreamHelper.Header, 0, dataWithHeader, 0, GZipStreamHelper.Header.Length);
                Array.Copy(data, 0, dataWithHeader, GZipStreamHelper.Header.Length, data.Length);
                using(var inStream = new GZipStream(new MemoryStream(dataWithHeader), CompressionMode.Decompress))
                {
                    using(var outStream = new MemoryStream(BitConverter.ToInt32(data, data.Length - sizeof(Int32))))
                    {
                        inStream.CopyTo(outStream);
                        return outStream.ToArray();
                    }
                }
            }
            catch(Exception e)
            {
                throw new ApplicationException("Failed to decompress block", e);
            }
        }

        protected sealed override void WriteBlock(Stream output, byte[] data)
        {
            try
            {
                output.Write(data, 0, data.Length);
                output.Flush();
            }
            catch(Exception e)
            {
                throw new ApplicationException("Failed to write block to output file", e);
            }
        }

        public sealed override void Process(Stream input, Stream output)
        {
            try
            {
                base.Process(input, output);
            }
            catch(Exception e)
            {
                throw new ApplicationException("Failed to decompress", e);
            }
        }
    };
}