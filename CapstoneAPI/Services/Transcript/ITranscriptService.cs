using CapstoneAPI.DataSets.Transcript;
using CapstoneAPI.DataSets.User;
using CapstoneAPI.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Services.Transcript
{
    public interface ITranscriptService
    {
        Task<Response<IEnumerable<UserTranscriptTypeDataSet>>> GetMarkOfUser(string token);
    }
}
