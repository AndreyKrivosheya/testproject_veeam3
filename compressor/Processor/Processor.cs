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
    abstract class Processor
    {
        public Processor(SettingsProvider settings)
        {
            this.Settings = settings;
        }
        
        protected readonly SettingsProvider Settings;
         
        protected abstract byte[] ReadBlock(Stream input);
        protected abstract byte[] ConvertBlock(byte[] data);
        protected abstract void WriteBlock(Stream output, byte[] data);

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
                    var block = ReadBlock(input);
                    if(block != null)
                    {
                        var eventThisBlockWritten = new ManualResetEvent(false);
                        threadPool.Queue(cancellationOnError, (block, eventPreviousBlockWritten, eventThisBlockWritten) => {
                            try
                            {
                                try
                                {
                                    // convert block (compress/decompress)
                                    var blockConverted = ConvertBlock(block);
                                    // wait previous block was written to maintain order
                                    if(eventPreviousBlockWritten != null)
                                        eventPreviousBlockWritten.WaitOneAndDispose(cancellationOnError.Token);
                                    // write this block
                                    WriteBlock(output, blockConverted);
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