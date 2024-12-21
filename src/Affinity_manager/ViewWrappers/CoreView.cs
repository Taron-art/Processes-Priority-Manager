using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Affinity_manager.ViewWrappers
{
    public partial class CoreView : ObservableObject, IComparable<CoreView>
    {
        [ObservableProperty]
        private bool _value;

        [ObservableProperty]
        private string _label;

        public CoreView(bool value, string label)
        {
            _value = value;
            _label = label;
        }

        public int CompareTo(CoreView? other)
        {
            if (other == null) return 1;
            return Label.CompareTo(other.Label);
        }
    }
}
