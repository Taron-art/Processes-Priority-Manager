using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Affinity_manager.ViewWrappers
{
    internal class ProcessConfigurationEqualityComparer : IEqualityComparer<ProcessConfigurationView>
    {
        public bool Equals(ProcessConfigurationView? x, ProcessConfigurationView? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(y, null)) return false;
            if (ReferenceEquals(x, null)) return false;

            return x.ProcessConfiguration.Name.Equals(y.ProcessConfiguration.Name, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode([DisallowNull] ProcessConfigurationView obj)
        {
            return obj.ProcessConfiguration.Name.GetHashCode(StringComparison.OrdinalIgnoreCase);
        }
    }
}
