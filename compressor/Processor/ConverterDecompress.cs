using System;
using System.IO;
using System.IO.Compression;

using compressor.Processor.Settings;

namespace compressor.Processor
{
    class ConverterDecompress: Converter
    {
        public ConverterDecompress(SettingsProvider settings)
            : base(settings)
        {
        }

        public sealed override byte[] Convert(byte[] data)
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
    }
}