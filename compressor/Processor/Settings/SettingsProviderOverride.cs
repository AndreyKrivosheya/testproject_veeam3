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

        public SettingsProviderOverride(SettingsProvider settings, int? concurrencyNoMoreThan = null)
        {
            BlockSizeLazy = new Lazy<long>(() => {
                    var value = settings.BlockSize;
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