using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Repositories
{
    public interface IUnitOfWork
    {
        Task<int> CommitAsync();
    }
}
