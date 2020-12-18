using System;

namespace compressor.Processor.Settings
{
    class SettingsProviderFromEnvironment : SettingsProvider
    {
        static string ReadFromEnvironmentVariable(string environmentVariableName)
        {
            var value = Environment.GetEnvironmentVariable(environmentVariableName);
            if(string.IsNullOrEmpty(value))
            {
                return null;
            }
            else
            {
                return Environment.ExpandEnvironmentVariables(value);
            }
        }
        static T ReadFromEnvironmentVariableAndConvertToT<T>(string environmentVariableName, T defaultValue, Func<string, T> converter)
        {
            var value = ReadFromEnvironmentVariable(environmentVariableName);
            if(string.IsNullOrEmpty(value))
            {
                return defaultValue;
            }
            else
            {
                try
                {
                    return converter(value);
                }
                catch(Exception)
                {
                    return defaultValue;
                }
            }
        }
        static long ReadFromEnvironmentVariableAndConvertToLong(string environmentVariableName, long defaultValue)
        {
            return ReadFromEnvironmentVariableAndConvertToT(environmentVariableName, default, (s) => long.Parse(s));
        }
        static int ReadFromEnvironmentVariableAndConvertToInt(string environmentVariableName, int defaultValue)
        {
            return ReadFromEnvironmentVariableAndConvertToT(environmentVariableName, defaultValue, (s) => int.Parse(s));
        }

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

        protected SettingsProviderFromEnvironment()
        {
            BlockSizeLazy = new Lazy<long>(() => {
                    var def = 1L * 1024 * 1024;
                    var
                    value = ReadFromEnvironmentVariableAndConvertToLong("COMPRESSOR_BLOCK_SIZE", def);
                    value = Math.Min(value >= 1 ? value : def, int.MaxValue);
                    return value;
                });
            MaxConcurrencyLazy = new Lazy<int>(() => {
                    var def = Environment.ProcessorCount;
                    var
                    value = ReadFromEnvironmentVariableAndConvertToInt("COMPRESSOR_MAX_CONCURRENCY", def);
                    value = value >= 1 ? value : def;
                    return value;
                });
        }

        static readonly Lazy<SettingsProvider> InstanceLazy = new Lazy<SettingsProvider>(() => new SettingsProviderFromEnvironment());
        public static SettingsProvider Instance
        {
            get
            {
                return InstanceLazy.Value;
            }
        }
    }
}