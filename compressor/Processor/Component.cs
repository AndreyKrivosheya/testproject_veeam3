using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Runtime.ExceptionServices;

using compressor.Common;
using compressor.Common.Threading;
using compressor.Processor.Settings;
using compressor.Processor.Utils;

namespace compressor.Processor
{
    abstract class Component
    {
        public Component(SettingsProvider settings)
        {
            this.Settings = settings;
        }
         
        protected readonly SettingsProvider Settings;
    }
}