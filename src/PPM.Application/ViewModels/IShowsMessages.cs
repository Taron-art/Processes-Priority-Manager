using System;

namespace Affinity_manager.ViewModels
{
    public interface IShowsMessages
    {
        public event EventHandler<string>? ShowMessage;
    }
}
