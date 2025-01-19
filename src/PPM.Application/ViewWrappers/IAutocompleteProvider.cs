using System.Collections.Generic;

namespace Affinity_manager.ViewWrappers
{
    public interface IAutocompleteProvider
    {
        void AddProcesses(IEnumerable<string> value);
        void ClearCache();
        ProcessInfoView[] GetAutocompleteList(string? searchString);
    }
}