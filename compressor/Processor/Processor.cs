using System;
using System.IO;
using System.Threading;

using compressor.Common;
using compressor.Common.Threading;
using compressor.Processor.Settings;

namespace compressor.Processor
{
    abstract class Processor: Component
    {
        public Processor(SettingsProvider settings, Reader.Factory readerFactory, Converter.Factory converterFactory, Writer.Factory writerFactory)
            : base(settings)
        {
            this.ReaderFactory = readerFactory;
            this.ConverterFactory = converterFactory;
            this.WriterFactory = writerFactory;
        }
        
        readonly Reader.Factory ReaderFactory;
        readonly Converter.Factory ConverterFactory;
        readonly Writer.Factory WriterFactory;

        public virtual void Process(Stream input, Stream output)
        {
            var reader = ReaderFactory(Settings);
            var converter = ConverterFactory(Settings);
            var writer = WriterFactory(Settings);

            Errors errors = new Errors();
            using(var cancellationOnError = new CancellationTokenSource())
            using(var threadPool = new CustomThreadPool(Settings.MaxConcurrency))
            {
                ManualResetEvent _eventPreviousBlockWritten = null;
                while(true)
                {
                    try
                    {
                        cancellationOnError.Token.ThrowIfCancellationRequested();
                        // read next block and process
                        var block = reader.ReadBlock(input);
                        if(block != null)
                        {
                            var eventThisBlockWritten = new ManualResetEvent(false);
                            var eventPreviousBlockWritten = _eventPreviousBlockWritten; // bellow closure would capture different values each cycle repeat
                            // convert and write async
                            threadPool.Queue(cancellationOnError.Token, () => {
                                try
                                {
                                    // convert block (compress/decompress)
                                    var blockConverted = converter.ConvertBlock(block);
                                    // wait previous block was written to maintain order
                                    if(eventPreviousBlockWritten != null)
                                        eventPreviousBlockWritten.WaitOneAndDispose(cancellationOnError.Token);
                                    // write this block
                                    writer.WriteBlock(output, blockConverted);
                                    // notify this block is written
                                    eventThisBlockWritten.Set();
                                }
                                catch(Exception e)
                                {
                                    if(!(e is OperationCanceledException) || !cancellationOnError.IsCancellationRequested)
                                    {
                                        errors.Add(e);
                                        cancellationOnError.Cancel();
                                    }
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
                try
                {
                    // previous block written event becomes last block written event
                    // when all blocks are queued for compression/decompression and writing 
                    if(_eventPreviousBlockWritten != null)
                        _eventPreviousBlockWritten.WaitOneAndDispose(cancellationOnError.Token);
                }
                catch(OperationCanceledException)
                {
                    if(!cancellationOnError.IsCancellationRequested)
                    {
                        throw;
                    }
                }
                // report errors if any
                errors.Throw();
            }
        }
        
        public virtual void Process(string input, string output)
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
    };
}