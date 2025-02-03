using System;
using CommunityToolkit.Mvvm.ComponentModel;
using PPM.Unsafe;

namespace Affinity_manager.ViewWrappers.Affinity
{
    public abstract partial class CoreGroupView<T> : ObservableObject, ICoreView where T : CoreGroup
    {
        [ObservableProperty]
        public bool? _selected;

        public required T CoreGroup { get; init; }

        public abstract string Label { get; }

        public string? Description { get; } = null;
    }
}
