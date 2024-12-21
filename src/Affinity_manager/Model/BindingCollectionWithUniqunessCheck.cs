using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Affinity_manager.Model
{
    public class BindingCollectionWithUniqunessCheck<T> : ObservableCollection<T> where T : INotifyPropertyChanged, IComparable<T>
    {
        private IEqualityComparer<T> _equalityComparer;

        public BindingCollectionWithUniqunessCheck(IEqualityComparer<T>? equalityComparer = null)
        {
            _equalityComparer = equalityComparer ?? EqualityComparer<T>.Default;
        }

        public BindingCollectionWithUniqunessCheck(IEnumerable<T> items, IEqualityComparer<T>? equalityComparer = null) : base(items.Order())
        {
            _equalityComparer = equalityComparer ?? EqualityComparer<T>.Default;

            if (items.Distinct(_equalityComparer).Count() != Count)
            {
                throw new ArgumentException("Collection contains non-unique elements", nameof(items));
            }

            foreach (T item in this)
            {
                RegisterItemEvents(item);
            }
        }

        public bool TryAddItem(T item)
        {
            if (this.Contains(item, _equalityComparer))
            {
                return false;
            }

            Add(item);
            return true;
        }

        protected override void ClearItems()
        {
            foreach (T item in this)
                UnregisterItemEvents(item);

            base.ClearItems();
        }

        protected override void InsertItem(int index, T item)
        {
            if (this.Contains(item, _equalityComparer))
            {
                throw new ArgumentException("The collection already contains this element", nameof(item));
            }

            int i;
            for (i = 0; i < Count; i++)
            {
                int compareResult = this[i]?.CompareTo(item) ?? 0;
                if (compareResult >= 0)
                {
                    break;
                }
            }

            base.InsertItem(i, item);

            RegisterItemEvents(item);
        }

        protected override void RemoveItem(int index)
        {
            UnregisterItemEvents(this[index]);

            base.RemoveItem(index);
        }

        private void RegisterItemEvents(T item)
        {
            if (item != null)
            {
                item.PropertyChanged += OnItemPropertyChanged;
            }
        }

        private void UnregisterItemEvents(T item)
        {
            if (item != null)
            {
                item.PropertyChanged -= OnItemPropertyChanged;
            }
        }

        private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            ItemChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler<EventArgs>? ItemChanged;
    }
}
