using System;
using System.IO;
using System.IO.Compression;

using compressor.Processor.Settings;

namespace compressor.Processor
{
    class ConverterDecompress : Converter
    {
        public ConverterDecompress(SettingsProvider settings)
            : base(settings)
        {
        }
        
        public sealed override byte[] ConvertBlock(byte[] data)
        {
            try
            {
                using(var inStream = new GZipStream(new MemoryStream(data), CompressionMode.Decompress))
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
    };
}