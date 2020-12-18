using System;

namespace compressor.Processor.Settings
{
    class SettingsProviderOverride : SettingsProvider
    {
        readonly Lazy<int> MaxConcurrencyLazy;
        public int MaxConcurrency
        {
            get
            {
                return MaxConcurrencyLazy.Value;
            }
        }

        readonly Lazy<long> BlockSizeLazy;
        public long BlockSize
        {
            get
            {
                return BlockSizeLazy.Value;
            }
        }

        public SettingsProviderOverride(SettingsProvider settings, int? concurrencyNoMoreThan = null, int? blockSizeNoLessThan = null)
        {
            BlockSizeLazy = new Lazy<long>(() => {
                    var value = settings.BlockSize;
                    if(blockSizeNoLessThan.HasValue)
                    {
                        if(blockSizeNoLessThan.Value > 1)
                        {
                            value = Math.Max(blockSizeNoLessThan.Value, value);
                        }
                    }
                    return value;
                });
            MaxConcurrencyLazy = new Lazy<int>(() => {
                    var value = settings.MaxConcurrency;
                    if(concurrencyNoMoreThan.HasValue)
                    {
                        if(concurrencyNoMoreThan.Value > 1)
                        {
                            value = Math.Min(value, concurrencyNoMoreThan.Value);
                        }
                    }

                    return value;
                });
        }
    }
}