using System.IO;

using compressor.Processor.Settings;

namespace compressor.Processor
{
    abstract class Reader
    {
        public Reader(SettingsProvider settings, Stream streamToRead)
        {
            this.Settings = settings;
            this.StreamToRead = streamToRead;
        }

        readonly protected SettingsProvider Settings;
        readonly protected Stream StreamToRead;

        public abstract byte[] ReadBlock();
    }
}