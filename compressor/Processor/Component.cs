using compressor.Processor.Settings;

namespace compressor.Processor
{
    abstract class Component
    {
        public Component(SettingsProvider settings)
        {
            this.Settings = settings;
        }
        
        protected readonly SettingsProvider Settings;
    };
}