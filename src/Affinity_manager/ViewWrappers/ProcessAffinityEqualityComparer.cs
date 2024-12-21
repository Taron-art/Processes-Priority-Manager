using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Affinity_manager.ViewWrappers
{
    internal class ProcessAffinityEqualityComparer : IEqualityComparer<ProcessAffinityView>
    {
        public bool Equals(ProcessAffinityView? x, ProcessAffinityView? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(y, null)) return false;
            if (ReferenceEquals(x, null)) return false;

            return x.ProcessAffinity.Name.Equals(y.ProcessAffinity.Name, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode([DisallowNull] ProcessAffinityView obj)
        {
            return obj.ProcessAffinity.Name.GetHashCode(StringComparison.OrdinalIgnoreCase);
        }
    }
}
