using System;
using System.IO;

using compressor.Processor.Settings;

namespace compressor.Processor
{
    class ReaderFromFile : Reader
    {
        public ReaderFromFile(SettingsProvider settings, ReadingStrategy readingStrategy = null)
            : base(settings, readingStrategy)
        {
        }
        
        public sealed override byte[] ReadBlock(Stream input)
        {
            try
            {
                return ReadingStrategy.ReadBytes(input, Settings.BlockSize);
            }
            catch(Exception e)
            {
                throw new ApplicationException("Failed to read block from input file", e);
            }
        }
    };
}