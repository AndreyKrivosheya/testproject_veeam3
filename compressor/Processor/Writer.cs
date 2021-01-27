using System.Collections.Generic;
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
            this.WritingStrategy = writingStrategy ?? WritingStrategy.Default(settings);
        }
        
        protected readonly WritingStrategy WritingStrategy;

        protected abstract void WriteBlock(Stream output, byte[] data, bool flush);
        public void WriteBlock(Stream output, byte[] data)
        {
            WriteBlock(output, data, true);
        }
        public void WriteBlocks(Stream output, IEnumerable<byte[]> datas)
        {
            foreach(var data in datas)
            {
                WriteBlock(output, data, false);
            }
            WritingStrategy.Flush(output);
        }
    };
}