using System;
using System.IO;

using compressor.Processor.Settings;

namespace compressor.Processor
{
    abstract class ReadingStrategy : Component
    {
        public ReadingStrategy(SettingsProvider settings)
            : base(settings)
        {
        }
        
        public abstract byte[] ReadBytes(Stream input, long count, bool exactly = false);
    };
}