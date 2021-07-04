using CapstoneAPI.Features.Transcript.DataSet;
using CapstoneAPI.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.Transcript.Service
{
    public interface ITranscriptService
    {
        Task<Response<IEnumerable<UserTranscriptTypeDataSet>>> GetMarkOfUser(string token);
    }
}
