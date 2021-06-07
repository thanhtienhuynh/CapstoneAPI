using CapstoneAPI.DataSets.Transcript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Repositories.Transcript
{
    public interface ITranscriptRepository : IGenericRepository<Models.Transcript>
    {
        Task<IEnumerable<UserTranscriptTypeDataSet>> GetUserTranscripts(int userId);
    }
}
