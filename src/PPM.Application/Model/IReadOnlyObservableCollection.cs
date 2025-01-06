using System.Collections.Generic;
using System.Collections.Specialized;

namespace Affinity_manager.Model
{
    public interface IReadOnlyObservableCollection<T> : IReadOnlyCollection<T>, INotifyCollectionChanged
    {
    }
}
