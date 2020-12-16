using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

using compressor.Processor.Settings;

namespace compressor.Processor
{
    abstract class Writer: Component
    {
        public Writer(SettingsProvider settings) : base(settings)
        {
        }

        public abstract void WriteBlock(Stream output, byte[] data);
   }
}