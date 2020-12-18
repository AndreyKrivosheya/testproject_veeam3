using System;
using System.IO;
using System.IO.Compression;

using compressor.Processor.Settings;

namespace compressor.Processor
{
    class ConverterCompress : Converter
    {
        public ConverterCompress(SettingsProvider settings)
            : base(settings)
        {
        }
        
        public sealed override byte[] ConvertBlock(byte[] data)
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

                    return outStreamRaw.ToArray();
                }
            }
            catch(Exception e)
            {
                throw new ApplicationException("Failed to compress block", e);
            }
        }
    };
}