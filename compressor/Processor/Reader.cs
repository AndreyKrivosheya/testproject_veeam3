using System.IO;

using compressor.Processor.Settings;

namespace compressor.Processor
{
    abstract class Reader : Component
    {
        public Reader(SettingsProvider settings) : base(settings)
        {
        }

        public abstract byte[] ReadBlock(Stream input);
    }
}