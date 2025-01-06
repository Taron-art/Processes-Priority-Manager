using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Affinity_manager.Model.CRUD
{
    public interface IProcessConfigurationsRepository
    {
        List<ProcessConfiguration> Get();
        Task SaveAsync(IEnumerable<ProcessConfiguration> items, Func<Task>? readyToGetCallback = null);
    }
}