namespace Affinity_manager.Model
{
    public interface IProcessConfigurationApplier
    {
        bool ApplyIfPresent(byte processorCount, ProcessConfiguration configuration);
    }
}
