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
        static T ReadFromEnvironmentVariableAndConvertToT<T>(string environmentVariableName, T defaultValue, Func<string, T> convertor)
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
                    return convertor(value);
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
                    var value = ReadFromEnvironmentVariableAndConvertToLong("COMPRESSOR_BLOCK_SIZE", def);
                    return Math.Min(value >= 1 ? value : def, int.MaxValue);
                });
            MaxConcurrencyLazy = new Lazy<int>(() => {
                    var def = Environment.ProcessorCount;
                    var value = ReadFromEnvironmentVariableAndConvertToInt("COMPRESSOR_MAX_CONCURRENCY", def);
                    return value >= 1 ? value : def;
                });
        }

        static readonly Lazy<SettingsProviderFromEnvironment> InstanceLazy = new Lazy<SettingsProviderFromEnvironment>(() => new SettingsProviderFromEnvironment());
        public static SettingsProviderFromEnvironment Instance
        {
            get
            {
                return InstanceLazy.Value;
            }
        }
    }
}