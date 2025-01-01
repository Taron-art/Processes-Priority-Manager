using System;

namespace Affinity_manager.ViewModels
{
    internal interface IShowsMessages
    {
        public event EventHandler<string>? ShowMessage;
    }
}
