using System;
using System.Threading;

using compressor.Processor.Settings;

namespace compressor.Processor
{
    abstract class Converter
    {
        public Converter(SettingsProvider settings)
        {
            this.Settings = settings;
        }

        readonly protected SettingsProvider Settings;

        public abstract byte[] Convert(byte[] data);
    }
}