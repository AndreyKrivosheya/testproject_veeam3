using System;
using System.IO;
using System.IO.Compression;

using compressor.Processor.Settings;

namespace compressor.Processor
{
    class ProcessorCompress: Processor
    {
        public ProcessorCompress(SettingsProvider settings)
            : base(settings)
        {
        }
         
        protected sealed override byte[] ReadBlock(Stream input)
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
        
        protected sealed override byte[] ConvertBlock(byte[] data)
        {
            try
            {
                using(var outStreamRaw = new MemoryStream())
                {
                    using(var outStreamRawGz = new GZipStream(outStreamRaw, CompressionLevel.Optimal, true))
                    {
                        using(var inStream = new MemoryStream(data))
                        {
                            inStream.CopyTo(outStreamRawGz);
                        }
                        outStreamRawGz.Flush();
                    }

                    if(outStreamRaw.Length >= GZipStreamHelper.Header.Length)
                    {
                        if(outStreamRaw.Length == GZipStreamHelper.Header.Length)
                        {
                            return new byte[] { };
                        }
                        else
                        {
                            // all bytes but the header
                            var compressedOriginal = outStreamRaw.ToArray();
                            var compressedHeaderStripped = new byte[compressedOriginal.Length - GZipStreamHelper.Header.Length];
                            Array.Copy(compressedOriginal, GZipStreamHelper.Header.Length, compressedHeaderStripped, 0, compressedHeaderStripped.Length);

                            return compressedHeaderStripped;
                        }
                    }
                    else
                    {
                        throw new ApplicationException("Failed to compress block: compressed block size is less than header size");
                    }
                }
            }
            catch(Exception e)
            {
                throw new ApplicationException("Failed to compress block", e);
            }
        }

        protected sealed override void WriteBlock(Stream output, byte[] data)
        {
            // write block data length
            try
            {
                var blockLengthBuffer = BitConverter.GetBytes((Int64)data.Length);
                output.Write(blockLengthBuffer, 0, blockLengthBuffer.Length);
            }
            catch(Exception e)
            {
                throw new ApplicationException("Failed to write block length to archive", e);
            }
            // write block data
            try
            {
                output.Write(data, 0, data.Length);
                output.Flush();
            }
            catch(Exception e)
            {
                throw new ApplicationException("Failed to write block to archive", e);
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
                throw new ApplicationException("Failed to compress", e);
            }
        }
    };
}