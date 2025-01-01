using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

namespace Affinity_manager.ViewWrappers
{
    public class EnumViewWrapper<T> where T : struct, Enum
    {
        private readonly ResourceLoader _resource;
        private readonly string _resourceIdentifier;

        public EnumViewWrapper(T value) 
        {
            Value = value;
            _resource = ResourceLoader.GetForViewIndependentUse();
            _resourceIdentifier = $"{typeof(T).Name}/{Enum.GetName(Value)}";
        }

        public T Value { get; }

        public string DisplayName
        {
            get
            {
                var displayName = _resource.GetString(_resourceIdentifier);
                if (string.IsNullOrEmpty(displayName))
                {
                    throw new KeyNotFoundException($"Value for {_resourceIdentifier} was not found.");
                }
                return displayName;
            }
        }

        public static string ValuePath => nameof(Value);

        public override bool Equals(object? obj)
        {
            if (obj is EnumViewWrapper<T> value)
            {
                return Value.Equals(value.Value);
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
