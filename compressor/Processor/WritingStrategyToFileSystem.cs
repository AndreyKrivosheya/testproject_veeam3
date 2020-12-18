using System;
using System.IO;

using compressor.Processor.Settings;

namespace compressor.Processor
{
    class WritingStrategyToFileSystem : WritingStrategy
    {
        public WritingStrategyToFileSystem(SettingsProvider settings)
            : base(settings)
        {
        }
        
        public override void WriteBytes(Stream output, byte[] data, long offset)
        {
            output.Write(data, (int)offset, (int)(data.Length - offset));
        }
        public override void Flush(Stream output)
        {
            output.Flush();
        }
    };
}