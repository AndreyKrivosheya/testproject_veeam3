using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

using compressor.Processor.Settings;

namespace compressor.Processor
{
    abstract class Writer
    {
        public Writer(SettingsProvider settings, Stream streamToWrite)
        {
            this.Settings = settings;
            this.StreamToWrite = streamToWrite;
        }

        readonly protected SettingsProvider Settings;
        readonly protected Stream StreamToWrite;

        public abstract void WriteBlock(byte[] data);
   }
}