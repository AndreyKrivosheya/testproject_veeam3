using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using compressor.Common;
using compressor.Processor.Settings;

namespace compressor.Processor
{
    class ReaderFromArchiveWithoutBlockSizes : Reader
    {
        public ReaderFromArchiveWithoutBlockSizes(SettingsProvider settings, ReadingStrategy readingStrategy = null)
            : base(new SettingsProviderOverride(settings, blockSizeNoLessThan: GZipStreamHelper.Header.Length), readingStrategy)
        {
        }
        
        private class Buffer
        {
            public Buffer(byte[] data)
            {
                this.Data = data;
                this.Offset = 0;
            }

            public readonly byte[] Data;
            public int Offset;

            public int LengthPending
            {
                get
                {
                    return Data.Length - Offset;
                }
            }
        }
        readonly LinkedList<Buffer> Pending = new LinkedList<Buffer>();
        
        bool allRead = false;
        public sealed override byte[] ReadBlock(Stream input)
        {
            try
            {
                // read next block
                long blockLength = -1;
                while(true)
                {
                    // get next bytes
                    Buffer buffer = null;
                    // ... from pending if available block is just starting
                    if(blockLength == -1)
                    {
                        blockLength = 0;
                        if(Pending.Count > 0)
                        {
                            if(Pending.Count > 1)
                            {
                                blockLength = Pending.Select(b => b.LengthPending).Sum();
                                buffer = null;
                            }
                            else
                            {
                                buffer = Pending.First.Value;
                            }
                        }
                    }
                    // ... else from stream
                    if(buffer == null)
                    {
                        if(allRead)
                        {
                            break;
                        }
                        else
                        {
                            buffer = new Buffer(ReadingStrategy.ReadBytes(input, Settings.BlockSize));
                            if(buffer == null || buffer.Data == null)
                            {
                                // all read
                                allRead = true;
                                break;
                            }
                            else
                            {
                                // append buffer to pending
                                Pending.AddLast(buffer);
                            }
                        }
                    }

                    // detect block end by finding GZipStream magic header
                    {
                        var gzipStreamHeaderIndex = -1;
                        {
                            // start just after previously found gzip header if any
                            var startFrom = buffer.Offset + (buffer.Offset > 0 ? GZipStreamHelper.Header.Length : 0);
                            foreach(var i in buffer.Data.IndexOf(GZipStreamHelper.Header, startFrom))
                            {
                                if(i != buffer.Offset || blockLength > 0)
                                {
                                    gzipStreamHeaderIndex = i;
                                    break;
                                }
                            }
                        }
                        // if no gzip header is found in buffer ...
                        if(gzipStreamHeaderIndex == -1)
                        {
                            // ... advance block length by whole buffer length
                            blockLength += buffer.LengthPending;
                            // ... and continue reading
                            continue;
                        }
                        // else if gzip header is found in buffer ...
                        else
                        {
                            // ... advance block length up to gzip header found ...
                            blockLength += gzipStreamHeaderIndex - buffer.Offset;
                            // ... and break reading
                            break;
                        }
                    }
                }

                // if all read and no block was pending...
                if(blockLength <= 0)
                {
                    return null;
                }
                // else if block is pending ...
                else
                {
                    // ... copy all buffers pending up to total of blockLength bytes
                    var block = new byte[blockLength];
                    for(long blockLengthTransfered = 0; blockLengthTransfered < blockLength; )
                    {
                        var pending = Pending.First.Value;
                        // copy either whole buffer pending or only part up to next gzip header
                        var copyCount = Math.Min(pending.LengthPending, blockLength - blockLengthTransfered);
                        Array.Copy(pending.Data, pending.Offset, block, blockLengthTransfered, copyCount);
                        blockLengthTransfered += copyCount;
                        // if buffer was copied completely ...
                        if(pending.LengthPending == copyCount)
                        {
                            // ... remove from pending
                            Pending.RemoveFirst();
                        }
                        // else if buffer was copied up to next gzip header
                        else
                        {
                            // ... advance buffer offset so that next block
                            // ... start with gzip header
                            pending.Offset += (int)copyCount;
                        }
                    }
                    
                    return block;
                }
            }
            catch(Exception e)
            {
                throw new ApplicationException("Failed to read block from archive", e);
            }
        }
    };
}