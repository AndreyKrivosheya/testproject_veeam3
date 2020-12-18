using System.IO;

using compressor.Processor.Settings;

namespace compressor.Processor
{
    abstract class Converter : Component
    {
        public delegate Converter Factory(SettingsProvider settings);
        public static Converter Compress(SettingsProvider settings)
        {
            return new ConverterCompress(settings);
        }
        public static Converter Decompress(SettingsProvider settings)
        {
            return new ConverterDecompress(settings);
        }

        public Converter(SettingsProvider settings)
            : base(settings)
        {
        }
        
        public abstract byte[] ConvertBlock(byte[] data);
    };
}