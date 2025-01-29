using System.Collections.Generic;
using System.Linq;
using Affinity_manager.Model;

namespace Affinity_manager.ViewWrappers
{
    public class ProcessConfigurationViewFactory : IProcessConfigurationViewFactory
    {
        private readonly IOptionsProvider _optionsProvider;
        private readonly IEqualityComparer<ProcessConfigurationView> _comparer;
        private readonly IProcessConfigurationApplier _configurationApplier;

        public ProcessConfigurationViewFactory(IOptionsProvider optionsProvider, IEqualityComparer<ProcessConfigurationView> comparer, IProcessConfigurationApplier configurationApplier)
        {
            _optionsProvider = optionsProvider;
            _comparer = comparer;
            _configurationApplier = configurationApplier;
        }

        public ProcessConfigurationView Create(ProcessConfiguration configuration)
        {
            return new ProcessConfigurationView(configuration, _optionsProvider, _configurationApplier);
        }

        public BindingCollectionWithUniqunessCheck<ProcessConfigurationView> CreateCollection(IEnumerable<ProcessConfiguration> processConfigurations)
        {
            return new BindingCollectionWithUniqunessCheck<ProcessConfigurationView>(processConfigurations.Select(Create), _comparer);
        }
    }
}
