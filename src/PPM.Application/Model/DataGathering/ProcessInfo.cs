using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Affinity_manager.Model.DataGathering
{
    public partial class ProcessInfo : ObservableObject, IComparable<ProcessInfo>
    {
        protected const StringComparison ComparisonInfo = StringComparison.OrdinalIgnoreCase;
        protected string? _friendlyName;
        private string? _moduleFullPath;

        public ProcessInfo(string mainModuleName, Source source = Source.None)
        {
            ArgumentException.ThrowIfNullOrEmpty(mainModuleName, nameof(mainModuleName));

            MainModuleName = mainModuleName;
            Source = source;
        }

        public string? ModuleFullPath
        {
            get => _moduleFullPath;

            private set
            {
                if (_moduleFullPath != value)
                {
                    _moduleFullPath = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IconSourcePath));
                }
            }
        }

        public Source Source { get; }

        public virtual string MainModuleName { get; }

        public virtual string? FriendlyName
        {
            get
            {
                if (!string.IsNullOrEmpty(_friendlyName))
                {
                    return _friendlyName;
                }

                switch (Source)
                {
                    case Source.None:
                        return _friendlyName;
                    case Source.RunningTasks:
                        return Strings.PPM.BackgroundProcess;
                    case Source.ExistingProfiles:
                        return Strings.PPM.FromSavedProfile;
                    default:
                        throw new NotSupportedException(Source.ToString());
                }
            }
        }

        public virtual string? IconSourcePath { get => ModuleFullPath; }

        public virtual byte Rating
        {
            get
            {
                return (byte)(Convert.ToByte(!string.IsNullOrEmpty(IconSourcePath)) + Convert.ToByte(!string.IsNullOrEmpty(_friendlyName)));
            }
        }

        public bool Matches(string prefix)
        {
            return MainModuleName.StartsWith(prefix, ComparisonInfo) || (_friendlyName != null && _friendlyName.Contains(prefix, StringComparison.CurrentCultureIgnoreCase));
        }

        public void UpdateWithFriendlyNameAndModulePath(string? friendlyName, string? modulePath)
        {
            if (_friendlyName == null)
            {
                _friendlyName = friendlyName;
                OnPropertyChanged(nameof(FriendlyName));
            }
            if (ModuleFullPath == null)
            {
                ModuleFullPath = modulePath;
            }
        }

        public override string ToString()
        {
            return MainModuleName;
        }

        public override bool Equals(object? obj)
        {
            return obj is ProcessInfo info &&
                   MainModuleName.Equals(info.MainModuleName, ComparisonInfo);
        }

        public override int GetHashCode()
        {
            return MainModuleName.GetHashCode(ComparisonInfo);
        }

        public virtual int CompareTo(ProcessInfo? other)
        {
            if (other == null) return 1;
            return string.Compare(MainModuleName, other.MainModuleName, ComparisonInfo);
        }
    }
}
