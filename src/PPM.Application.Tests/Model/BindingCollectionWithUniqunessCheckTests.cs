using System;
using System.Collections.Generic;
using System.ComponentModel;
using Affinity_manager.Model;
using FluentAssertions;
using NUnit.Framework;

namespace PPM.Application.Tests.Model
{
    [TestFixture]
    public class BindingCollectionWithUniqunessCheckTests
    {
        public class TestItem : INotifyPropertyChanged, IComparable<TestItem>
        {
            public event PropertyChangedEventHandler? PropertyChanged;

            public int Value { get; set; }

            public int CompareTo(TestItem? other)
            {
                return Value.CompareTo(other?.Value);
            }

            public override bool Equals(object? obj)
            {
                return obj is TestItem item && Value == item.Value;
            }

            public override int GetHashCode()
            {
                return Value.GetHashCode();
            }

            public void RaiseOnPropertyChangedEvent(string propertyName)
            {
                PropertyChangedEventArgs e = new(propertyName);
                PropertyChanged?.Invoke(this, e);
            }
        }

        [Test]
        public void Constructor_ShouldThrowException_WhenItemsAreNotUnique()
        {
            var items = new List<TestItem>
            {
                new() { Value = 1 },
                new() { Value = 1 }
            };

            Assert.Throws<ArgumentException>(() => new BindingCollectionWithUniqunessCheck<TestItem>(items));
        }

        [Test]
        public void TryAddItem_ShouldReturnFalse_WhenItemAlreadyExists()
        {
            var item = new TestItem { Value = 1 };
            var item2 = new TestItem { Value = 1 };
            var collection = new BindingCollectionWithUniqunessCheck<TestItem>(new List<TestItem> { item });

            var result = collection.TryAddItem(item2);

            Assert.That(result, Is.False);
        }

        [Test]
        public void TryAddItem_ShouldReturnTrue_WhenItemIsUnique()
        {
            var item = new TestItem { Value = 1 };
            var item2 = new TestItem { Value = 2 };
            var collection = new BindingCollectionWithUniqunessCheck<TestItem>(new List<TestItem> { item });

            var result = collection.TryAddItem(item2);

            Assert.That(result, Is.True);
        }

        [Test]
        public void InsertItem_ShouldThrowException_WhenItemAlreadyExists()
        {
            var item = new TestItem { Value = 1 };
            var collection = new BindingCollectionWithUniqunessCheck<TestItem>(new List<TestItem> { item });

            Assert.Throws<ArgumentException>(() => collection.Add(item));
        }

        [Test]
        public void InsertItem_ShouldAddItemInSortedOrder()
        {
            var item1 = new TestItem { Value = 2 };
            var item2 = new TestItem { Value = 1 };
            var collection = new BindingCollectionWithUniqunessCheck<TestItem>();

            collection.Add(item1);
            collection.Add(item2);

            Assert.That(collection[0], Is.EqualTo(item2));
            Assert.That(collection[1], Is.EqualTo(item1));
        }

        [Test]
        public void ClearItems_ShouldUnregisterEvents()
        {
            var item = new TestItem();
            var collection = new BindingCollectionWithUniqunessCheck<TestItem>(new List<TestItem> { item });

            using var monitor = collection.Monitor();

            collection.Clear();
            item.RaiseOnPropertyChangedEvent(nameof(item.Value));

            monitor.Should().NotRaise(nameof(collection.ItemChanged));
        }

        [Test]
        public void RemoveItem_ShouldUnregisterEvents()
        {
            var item = new TestItem();
            var collection = new BindingCollectionWithUniqunessCheck<TestItem>(new List<TestItem> { item });

            using var monitor = collection.Monitor();

            collection.Remove(item);
            item.RaiseOnPropertyChangedEvent(nameof(item.Value));

            monitor.Should().NotRaise(nameof(collection.ItemChanged));
        }

        [Test]
        public void OnItemPropertyChanged_ShouldRaiseItemChangedEvent()
        {
            var item = new TestItem { Value = 1 };
            var collection = new BindingCollectionWithUniqunessCheck<TestItem>(new List<TestItem> { item });

            using (var monitor = collection.Monitor())
            {
                item.RaiseOnPropertyChangedEvent(nameof(item.Value));

                monitor.Should().Raise(nameof(collection.ItemChanged));
            }

            using (var monitor = collection.Monitor())
            {
                var item2 = new TestItem { Value = 2 };
                collection.Add(item2);
                item2.RaiseOnPropertyChangedEvent(nameof(TestItem.Value));
                monitor.Should().Raise(nameof(collection.ItemChanged));
            }
        }
    }
}
