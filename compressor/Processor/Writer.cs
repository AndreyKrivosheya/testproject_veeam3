using System.IO;

using compressor.Processor.Settings;

namespace compressor.Processor
{
    abstract class Writer : Component
    {
        public delegate Writer Factory(SettingsProvider settings);
        public static Writer.Factory ToFile = (settings) => new WriterToFile(settings);
        public static Writer.Factory ToArchive = (settings) => new WriterToArchive(settings);
        public static Writer.Factory ToArchiveWithoutBlockSizes = (settings) => new WriterToArchiveWithoutBlockSizes(settings);

        public Writer(SettingsProvider settings, WritingStrategy writingStrategy = null)
            : base(settings)
        {
            this.WritingStrategy = writingStrategy ?? new WritingStrategyToFileSystem(settings);
        }
        
        protected readonly WritingStrategy WritingStrategy;

        public abstract void WriteBlock(Stream output, byte[] data);
    };
}