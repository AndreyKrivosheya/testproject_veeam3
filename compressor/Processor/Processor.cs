using System;
using System.IO;
using System.Threading;

using compressor.Common;
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
         
        protected abstract byte[] ReadBlock(Stream input);
        protected abstract byte[] ConvertBlock(byte[] data);
        protected abstract void WriteBlock(Stream output, byte[] data);

        public virtual void Process(Stream input, Stream output)
        {
            if(input == output)
            {
                throw new ArgumentException("Can't process stream into itself");
            }
            else
            {
                Errors errors = new Errors();
                using(var cancellationOnError = new CancellationTokenSource())
                using(var threadPool = new CustomThreadPool(Settings.MaxConcurrency))
                {
                    ManualResetEventSlim _eventPreviousBlockWritten = null;
                    while(true)
                    {
                        try
                        {
                            cancellationOnError.Token.ThrowIfCancellationRequested();
                            // read next block and process
                            var block = ReadBlock(input);
                            if(block != null)
                            {
                                var eventThisBlockWritten = new ManualResetEventSlim(false);
                                var eventPreviousBlockWritten = _eventPreviousBlockWritten; // bellow closure would capture different values each cycle repeat
                                // convert and write async
                                threadPool.Queue(cancellationOnError.Token, () => {
                                    try
                                    {
                                        try
                                        {
                                            // convert block (compress/decompress)
                                            var blockConverted = ConvertBlock(block);
                                            // wait previous block was written to maintain order
                                            if(eventPreviousBlockWritten != null)
                                            {
                                                eventPreviousBlockWritten.WaitOneAndDispose(cancellationOnError.Token);
                                            }
                                            // write this block
                                            WriteBlock(output, blockConverted);
                                        }
                                        catch(Exception e)
                                        {
                                            if(!(e is OperationCanceledException) || !cancellationOnError.IsCancellationRequested)
                                            {
                                                errors.Add(e);
                                                cancellationOnError.Cancel();
                                            }
                                        }
                                    }
                                    finally
                                    {
                                        // notify this block is written
                                        eventThisBlockWritten.Set();
                                    }
                                });
                                // this block written event becomes previous block written event
                                _eventPreviousBlockWritten = eventThisBlockWritten;
                            }
                            else
                            {
                                // all read
                                break;
                            }
                        }
                        catch(Exception e)
                        {
                            if(!(e is OperationCanceledException) || !cancellationOnError.IsCancellationRequested)
                            {
                                errors.Add(e);
                                cancellationOnError.Cancel();
                                break;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                    // wait last block is written
                    // ... previous block written event becomes last block written event
                    // ... when all blocks are queued for compression/decompression and writing 
                    if(_eventPreviousBlockWritten != null)
                    {
                        _eventPreviousBlockWritten.WaitOneAndDispose();
                    }
                    // report errors if any
                    errors.Throw();
                }
            }
        }
        
        public virtual void Process(string input, string output)
        {
            if(input == output)
            {
                throw new ArgumentException("Can't process file into itself.");
            }
            else
            {
                FileStream outStream = null;
                try
                {
                    try
                    {
                        outStream = new FileStream(output, FileMode.Open);
                        outStream.SetLength(0);
                    }
                    catch(FileNotFoundException)
                    {
                        outStream = new FileStream(output, FileMode.CreateNew);
                    }

                    using(var inStream = new FileStream(input, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        Process(inStream, outStream);
                    }
                }
                finally
                {
                    if(outStream != null)
                    {
                        outStream.Dispose();
                        outStream = null;
                    }
                }
            }
        }
    };
}