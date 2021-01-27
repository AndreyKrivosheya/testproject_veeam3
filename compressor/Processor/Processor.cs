using System;
using System.Collections.Generic;
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
        protected abstract void WriteBlock(Stream output, byte[] data, bool flush);
        protected void WriteBlock(Stream output, byte[] data)
        {
            WriteBlock(output, data, true);
        }
        protected void WriteBlocks(Stream output, IEnumerable<byte[]> datas)
        {
            foreach(var data in datas)
            {
                WriteBlock(output, data, false);
            }
            output.Flush();
        }

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
                    ManualResetEventSlim _eventPreviousBlockProcessed = null;
                    List<byte[]> blocksToWrite = new List<byte[]>(Settings.MaxConcurrency);
                    while(true)
                    {
                        try
                        {
                            cancellationOnError.Token.ThrowIfCancellationRequested();
                            // read next block and process
                            var block = ReadBlock(input);
                            if(block != null)
                            {
                                var eventThisBlockProcessed = new ManualResetEventSlim(false);
                                var eventPreviousBlockProcessed = _eventPreviousBlockProcessed; // bellow closure would capture different values each cycle repeat
                                // convert and write async
                                threadPool.Queue(cancellationOnError.Token, () => {
                                    bool eventThisBlockProcessedWasSet = false;
                                    try
                                    {
                                        try
                                        {
                                            // convert block (compress/decompress)
                                            var blockConverted = ConvertBlock(block);
                                            // wait previous block was written to maintain order
                                            if(eventPreviousBlockProcessed != null)
                                            {
                                                eventPreviousBlockProcessed.WaitOneAndDispose(cancellationOnError.Token);
                                            }
                                            // queue this block for writing
                                            blocksToWrite.Add(blockConverted);
                                            // write blocks in bulck when queue is full
                                            if(blocksToWrite.Count >= blocksToWrite.Capacity)
                                            {
                                                var blocksToWriteCopy = new List<byte[]>(blocksToWrite);
                                                // notify this block is processed
                                                {
                                                    blocksToWrite.Clear();
                                                    // notify this block is processed
                                                    {
                                                        eventThisBlockProcessed.Set();
                                                        eventThisBlockProcessedWasSet = true;
                                                    }
                                                }
                                                WriteBlocks(output, blocksToWriteCopy);
                                            }
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
                                        // notify this block is processed
                                        if(!eventThisBlockProcessedWasSet)
                                        {
                                            eventThisBlockProcessed.Set();
                                        }
                                    }
                                });
                                // this block written event becomes previous block written event
                                _eventPreviousBlockProcessed = eventThisBlockProcessed;
                            }
                            else
                            {
                                // all read
                                {
                                    // wait last block is processed
                                    // ... previous block processed event becomes last processed written event
                                    // ... when all blocks are queued for compression/decompression and writing 
                                    if(_eventPreviousBlockProcessed != null)
                                    {
                                        _eventPreviousBlockProcessed.WaitOneAndDispose(cancellationOnError.Token);
                                    }
                                    // write blocks left, if any
                                    if(blocksToWrite.Count > 0)
                                    {
                                        WriteBlocks(output, blocksToWrite);
                                        blocksToWrite.Clear();
                                    }
                                }
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