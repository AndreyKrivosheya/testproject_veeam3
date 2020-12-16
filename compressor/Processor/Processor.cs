using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Runtime.ExceptionServices;

using compressor.Common.Threading;
using compressor.Processor.Settings;

namespace compressor.Processor
{
    abstract class Processor
    {
        public Processor(SettingsProvider settings)
        {
            this.Settings = settings;
        }
         
        protected readonly SettingsProvider Settings;

        protected abstract Reader CreateReader(Stream input);
        protected abstract Converter CreateConverter();
        protected abstract Writer CreateWriter(Stream output);

        public virtual void Process(Stream input, Stream output)
        {
            var reader = CreateReader(input);
            var converter = CreateConverter();
            var writer = CreateWriter(output);

            using(var cancellationOnError = new CancellationTokenSource())
            using(var threads = new CustomThreadPool(Settings.MaxConcurrency))
            {
                ManualResetEvent eventPreviousBlockWritten = null;
                while(true)
                {
                    var block = reader.ReadBlock();
                    if(block != null)
                    {
                        var eventThisBlockWritten = new ManualResetEvent(false);
                        threads.Queue((block, eventPreviousBlockWritten, eventThisBlockWritten) => {
                            // convert block (compress/decompress)
                            var blockConverted = converter.Convert(block);
                            // wait previous block was written to maintain order
                            if(eventPreviousBlockWritten != null)
                            {
                                eventPreviousBlockWritten.WaitOne();
                                // not need it anymore
                                eventPreviousBlockWritten.Dispose();
                                eventPreviousBlockWritten = null;
                            }
                            // write this block
                            writer.WriteBlock(blockConverted);
                            // notify this block is written
                            eventThisBlockWritten.Set();
                        }, block, eventPreviousBlockWritten, eventThisBlockWritten);
                        eventPreviousBlockWritten = eventThisBlockWritten;
                    }
                    else
                    {
                        // when all blocks are queued for conversion and writing 
                        if(eventPreviousBlockWritten != null)
                        {
                            eventPreviousBlockWritten.WaitOne();
                            eventPreviousBlockWritten.Dispose();
                            eventPreviousBlockWritten = null;
                        }
                        break;
                    }
                }
            }
        }
    };
}