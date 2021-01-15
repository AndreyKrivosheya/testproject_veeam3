using System;
using System.IO;

using compressor.Processor.Settings;

namespace compressor.Processor
{
    abstract class ReadingStrategy : Component
    {
        public delegate ReadingStrategy Factory(SettingsProvider settings);
        public static ReadingStrategy.Factory FromFileSystem = (settings) => new ReadingStrategyFromFileSystem(settings);
        public static ReadingStrategy.Factory Default = FromFileSystem;

        public ReadingStrategy(SettingsProvider settings)
            : base(settings)
        {
        }
        
        public abstract byte[] ReadBytes(Stream input, long count, bool exactly = false);
    };
}