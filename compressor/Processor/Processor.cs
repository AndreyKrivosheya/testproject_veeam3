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
    abstract class Processor: Component
    {
        public Processor(SettingsProvider settings, Reader reader, Converter converter, Writer writer) : base(settings)
        {
            this.Reader = reader;
            this.Converter = converter;
            this.Writer = writer;
        }
         
        readonly Reader Reader;
        readonly Converter Converter;
        readonly Writer Writer;

        public virtual void Process(Stream input, Stream output)
        {
            Errors errors = new Errors();
            using(var cancellationOnError = new CancellationTokenSource())
            using(var threadPool = new CustomThreadPool(Settings.MaxConcurrency))
            {
                ManualResetEvent eventPreviousBlockWritten = null;
                while(true)
                {
                    // check if any errors happend
                    errors.Throw();
                    // read next block and process
                    var block = Reader.ReadBlock(input);
                    if(block != null)
                    {
                        var eventThisBlockWritten = new ManualResetEvent(false);
                        threadPool.Queue(cancellationOnError, (block, eventPreviousBlockWritten, eventThisBlockWritten) => {
                            try
                            {
                                try
                                {
                                    // convert block (compress/decompress)
                                    var blockConverted = Converter.Convert(block);
                                    // wait previous block was written to maintain order
                                    if(eventPreviousBlockWritten != null)
                                        eventPreviousBlockWritten.WaitOneAndDispose(cancellationOnError.Token);
                                    // write this block
                                    Writer.WriteBlock(output, blockConverted);
                                    // notify this block is written
                                    eventThisBlockWritten.Set();
                                }
                                catch(OperationCanceledException)
                                {
                                    if(!cancellationOnError.IsCancellationRequested)
                                        throw;
                                }
                            }
                            catch(Exception e)
                            {
                                errors.Add(e);
                                cancellationOnError.Cancel();
                            }
                        }, block, eventPreviousBlockWritten, eventThisBlockWritten);
                        eventPreviousBlockWritten = eventThisBlockWritten;
                    }
                    else
                    {
                        // check if any errors happend
                        errors.Throw();
                        // previous block written event becomes last block written event
                        // when all blocks are queued for conversion and writing 
                        if(eventPreviousBlockWritten != null)
                            eventPreviousBlockWritten.WaitOneAndDispose(cancellationOnError.Token);

                        break;
                    }
                }
            }
        }
    };
}