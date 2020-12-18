using System;
using System.IO;

using compressor.Processor.Settings;

namespace compressor.Processor
{
    abstract class WritingStrategy : Component
    {
        public WritingStrategy(SettingsProvider settings)
            : base(settings)
        {
        }
        
        public abstract void WriteBytes(Stream output, byte[] data, long offset = 0);
        public abstract void Flush(Stream output);
    };
}