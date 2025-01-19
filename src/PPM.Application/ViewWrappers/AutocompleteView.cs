using System.Collections.Generic;
using System.Linq;
using Affinity_manager.Model.DataGathering;

namespace Affinity_manager.ViewWrappers
{
    public class AutocompleteView : IAutocompleteProvider
    {
        private const int NumberOfItemsToDisplay = 10;
        private readonly ManualAutocompleteProvider _manualAutocompleteProvider = new();
        private readonly List<IProcessProvider> _autocompleteProviders = new(3);

        private readonly List<ProcessInfoView> _processViewsCache = new(NumberOfItemsToDisplay);

        public AutocompleteView(IApplicationIconsLoader applicationIconsLoader)
        {
            ApplicationIconsLoader = applicationIconsLoader;
            _autocompleteProviders.Add(_manualAutocompleteProvider);
        }

        public IReadOnlyList<IProcessProvider> AutocompleteProviders => _autocompleteProviders;

        public IApplicationIconsLoader ApplicationIconsLoader { get; }

        public ProcessInfoView[] GetAutocompleteList(string? searchString)
        {
            if (string.IsNullOrWhiteSpace(searchString))
            {
                return [];
            }

            return AutocompleteProviders.SelectMany(provider => provider.GetMatchedProcesses(searchString))
                .Order()
                .ThenByDescending(info => info.Rating)
                .Distinct()
                .Take(NumberOfItemsToDisplay)
                .Select(ExctractFromCacheOrCreateNew).ToArray();
        }

        public void ClearCache()
        {
            while (_processViewsCache.Count > 0)
            {
                ProcessInfoView cache = _processViewsCache[0];
                _processViewsCache.RemoveAt(0);
                cache.Dispose();
            }
        }

        public void AddProcesses(IEnumerable<string> value)
        {
            _manualAutocompleteProvider.AddProcesses(value);
        }

        internal void AddProcessProvider(IProcessProvider provider)
        {
            _autocompleteProviders.Add(provider);
        }

        private ProcessInfoView ExctractFromCacheOrCreateNew(ProcessInfo item)
        {
            ProcessInfoView? view = _processViewsCache.Find(view => ReferenceEquals(view.ProcessInfo, item));
            if (view == null)
            {
                view = new ProcessInfoView(item, ApplicationIconsLoader);
                _processViewsCache.Add(view);
            }

            return view;
        }
    }
}
