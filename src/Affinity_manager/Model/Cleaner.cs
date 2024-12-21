using System.Linq;

namespace Affinity_manager.Model
{
    internal class Cleaner
    {
        public void Clean()
        {
            ProcessAffinitiesManager processAffinitiesManager = new();
            ProcessAffinity[] processAffinities = processAffinitiesManager.LoadFromRegistry().ToArray();
            foreach (ProcessAffinity processAffinity in processAffinities)
            {
                processAffinity.Reset();
            }

            processAffinitiesManager.SaveToRegistry(processAffinities);
        }
    }
}
