using CapstoneAPI.Features.Transcript.DataSet;
using CapstoneAPI.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.Transcript.Repository
{
    public interface ITranscriptRepository : IGenericRepository<Models.Transcript>
    {
        Task<IEnumerable<UserTranscriptTypeDataSet>> GetUserTranscripts(int userId);
        Task<double> GetLiteratureTestMark(int userId);
    }
}
