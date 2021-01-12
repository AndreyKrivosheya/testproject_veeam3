using System;
using System.IO;
using System.Linq;

using compressor.Common;
using compressor.Processor;
using compressor.Processor.Settings;

namespace compressor
{
    class Program
    {
        static void Usage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("compressor.exe compress|decompress in out");
        }

        static void RunProcessor(string pathIn, string pathOut, Processor.Processor processor)
        {
            var
            stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();
            try
            {
                processor.Process(pathIn, pathOut);
            }
            finally
            {
                stopWatch.Stop();
                System.Diagnostics.Debug.WriteLine("Time took: '{0}'", stopWatch.Elapsed);
            }
        }
        static void RunCompressor(string pathIn, string pathOut)
        {
            var settings = SettingsProviderFromEnvironment.Instance;
            // adjust concurrency according with total blocks
            // if total blocks is less then concurrency intended
            var pathInBlocks = (int)Math.Round(((double)(new FileInfo(pathIn)).Length) / ((double)settings.BlockSize), 0, MidpointRounding.AwayFromZero);
            if(settings.MaxConcurrency > pathInBlocks)
            {
                settings = new SettingsProviderOverride(settings,
                    concurrencyNoMoreThan: (int)pathInBlocks);
            }

            RunProcessor(pathIn, pathOut, new ProcessorCompress(settings));
        }
        static void RunDecompressor(string pathIn, string pathOut)
        {
            RunProcessor(pathIn, pathOut, new ProcessorDecompress(SettingsProviderFromEnvironment.Instance));
        }

        static int Main(string[] args)
        {
            try
            {
                if(args.Length < 3 || args.Length > 3)
                {
                    Usage();
                    return 1;
                }
                else
                {
                    if(string.Equals("compress", args[0], StringComparison.InvariantCultureIgnoreCase))
                    {
                        RunCompressor(args[1], args[2]);
                        return 0;
                    }
                    else if(string.Equals("decompress", args[0], StringComparison.InvariantCultureIgnoreCase))
                    {
                        RunDecompressor(args[1], args[2]);
                        return 0;
                    }
                    else
                    {
                        Console.WriteLine("Command '{0}' is unknown.", args[0]);
                        Usage();
                        return 1;
                    }
                }
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
                Console.WriteLine(e.GetMessagesChain());
                return 1;
            }
        }
    }
}
