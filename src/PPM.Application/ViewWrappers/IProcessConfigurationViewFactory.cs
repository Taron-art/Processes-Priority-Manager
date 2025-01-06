using System.Collections.Generic;
using Affinity_manager.Model;

namespace Affinity_manager.ViewWrappers
{
    public interface IProcessConfigurationViewFactory
    {
        ProcessConfigurationView Create(ProcessConfiguration configuration);
        BindingCollectionWithUniqunessCheck<ProcessConfigurationView> CreateCollection(IEnumerable<ProcessConfiguration> processConfigurations);
    }
}