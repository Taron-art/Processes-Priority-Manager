using System.Collections.Specialized;
using System.ComponentModel;

namespace Affinity_manager.ViewWrappers.Affinity
{
    public interface ICoreView : INotifyPropertyChanged
    {
        public bool? Selected { get; set; }
        public string Label { get; }
        public string? Description { get; }
    }
}
