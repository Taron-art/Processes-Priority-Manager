using System.Collections.Generic;
using System.Linq;
using Affinity_manager.Model;

namespace Affinity_manager.ViewWrappers
{
    public class ProcessConfigurationViewFactory : IProcessConfigurationViewFactory
    {
        private readonly IOptionsProvider _optionsProvider;
        private readonly IEqualityComparer<ProcessConfigurationView> _comparer;

        public ProcessConfigurationViewFactory(IOptionsProvider optionsProvider, IEqualityComparer<ProcessConfigurationView> comparer)
        {
            _optionsProvider = optionsProvider;
            _comparer = comparer;
        }

        public ProcessConfigurationView Create(ProcessConfiguration configuration)
        {
            return new ProcessConfigurationView(configuration, _optionsProvider);
        }

        public BindingCollectionWithUniqunessCheck<ProcessConfigurationView> CreateCollection(IEnumerable<ProcessConfiguration> processConfigurations)
        {
            return new BindingCollectionWithUniqunessCheck<ProcessConfigurationView>(processConfigurations.Select(Create), _comparer);
        }
    }
}
