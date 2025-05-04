using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using PPM.Unsafe;

namespace Affinity_manager.ViewWrappers.Affinity
{
    public class GroupsView<T> where T : CoreGroup
    {
        private readonly List<CoreGroupView<T>> _groups;
        private bool _stateChanging;

        public GroupsView(params CoreGroupView<T>[] groups)
        {
            ArgumentNullException.ThrowIfNull(groups, nameof(groups));

            _groups = new List<CoreGroupView<T>>(groups);

            foreach (CoreGroupView<T> group in _groups)
            {
                group.PropertyChanged += OnGroupPropertyChanged;
            }
        }

        public bool ShouldBeDisplayed
        {
            get
            {
#if DEBUG
                return true;
#else
                return Groups.Count > 1;
#endif
            }
        }

        public IReadOnlyList<CoreGroupView<T>> Groups => _groups;

        internal void UpdateGroupsState(CoreView[] coreViews)
        {
            if (_stateChanging)
            {
                return;
            }

            foreach (CoreGroupView<T> group in _groups)
            {
                var relatedCoreViews = coreViews.Where(coreView => coreView.CoreInfo.AssociatedGroups.Contains(group.CoreGroup)).ToArray();
                if (relatedCoreViews.All(coreView => coreView.DefinetelySelected))
                {
                    group.Selected = true;
                }
                else if (relatedCoreViews.All(coreView => !coreView.DefinetelySelected))
                {
                    group.Selected = false;
                }
                else
                {
                    group.Selected = null;
                }
            }
        }

        private void OnGroupPropertyChanged(object? sender, PropertyChangedEventArgs args)
        {
            Debug.Assert(sender is CoreGroupView<T>);
            if (args.PropertyName == nameof(CoreGroupView<T>.Selected))
            {
                _stateChanging = true;
                try
                {
                    GroupChanged?.Invoke(this, new GroupChangedEventArgs<T>((CoreGroupView<T>)sender));
                }
                finally
                {
                    _stateChanging = false;
                }
            }
        }

        public event EventHandler<GroupChangedEventArgs<T>>? GroupChanged;
    }
}
