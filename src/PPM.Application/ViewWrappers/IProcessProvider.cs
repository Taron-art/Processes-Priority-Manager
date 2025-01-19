using System.Collections.Generic;

namespace Affinity_manager.Model.DataGathering
{
    public interface IProcessProvider
    {
        IEnumerable<ProcessInfo> GetMatchedProcesses(string searchString);
    }
}
