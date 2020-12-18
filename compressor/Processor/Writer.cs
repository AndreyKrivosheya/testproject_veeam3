using System.IO;

using compressor.Processor.Settings;

namespace compressor.Processor
{
    abstract class Writer : Component
    {
        public delegate Writer Factory(SettingsProvider settings);
        public static Writer ToFile(SettingsProvider settings)
        {
            return new WriterToFile(settings);
        }
        public static Writer ToArchive(SettingsProvider settings)
        {
            return new WriterToArchive(settings);
        }

        public Writer(SettingsProvider settings)
            : base(settings)
        {
        }
        
        public virtual void WriteBlock(Stream output, byte[] data)
        {
            output.Write(data, 0, data.Length);
            output.Flush();
        }
    };
}