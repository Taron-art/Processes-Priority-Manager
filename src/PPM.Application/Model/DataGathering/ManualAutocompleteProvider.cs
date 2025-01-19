using System.Collections.Generic;
using System.Linq;

namespace Affinity_manager.Model.DataGathering
{
    public sealed class ManualAutocompleteProvider : IProcessProvider
    {
        private readonly List<ProcessInfo> _processes = [];

        public void AddProcesses(IEnumerable<string> processes)
        {
            foreach (string process in processes.Where(process => !string.IsNullOrWhiteSpace(process)))
            {
                _processes.Add(new ProcessInfo(process, Source.ExistingProfiles));
            }
        }

        public IEnumerable<ProcessInfo> GetMatchedProcesses(string searchString)
        {
            return _processes.Where(process => process.Matches(searchString));
        }
    }
}
