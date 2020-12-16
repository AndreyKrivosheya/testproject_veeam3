namespace compressor.Processor.Settings
{
    interface SettingsProvider
    {
        int MaxConcurrency { get; }

        long BlockSize { get; }
    }
}