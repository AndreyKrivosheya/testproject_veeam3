using System;
using System.IO;

using compressor.Processor.Settings;

namespace compressor.Processor
{
    abstract class Reader : Component
    {
        public delegate Reader Factory(SettingsProvider settings);
        public static Reader.Factory FromFile = (settings) => new ReaderFromFile(settings);
        public static Reader.Factory FromArchive = (settings) => new ReaderFromArchive(settings);
        public static Reader.Factory FromArchiveWithoutBlockSizes = (settings) => new ReaderFromArchiveWithoutBlockSizes(settings);

        public Reader(SettingsProvider settings, ReadingStrategy readingStrategy = null)
            : base(settings)
        {
            this.ReadingStrategy = readingStrategy ?? new ReadingStrategyFromFileSystem(settings);
        }
        
        protected readonly ReadingStrategy ReadingStrategy;

        public abstract byte[] ReadBlock(Stream input);
    };
}