using System;
using System.Threading;

using compressor.Processor.Settings;

namespace compressor.Processor
{
    abstract class Converter: Component
    {
        public Converter(SettingsProvider settings) : base(settings)
        {
        }

        public abstract byte[] Convert(byte[] data);
    }
}