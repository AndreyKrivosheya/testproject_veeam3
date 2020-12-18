using System;
using System.IO;

using compressor.Processor.Settings;

namespace compressor.Processor
{
    class ReaderFromFile : Reader
    {
        public ReaderFromFile(SettingsProvider settings)
            : base(settings)
        {
        }
        
        public sealed override byte[] ReadBlock(Stream input)
        {
            try
            {
                return base.ReadBlock(input);
            }
            catch(Exception e)
            {
                throw new ApplicationException("Failed to read block from input file", e);
            }
        }
    };
}