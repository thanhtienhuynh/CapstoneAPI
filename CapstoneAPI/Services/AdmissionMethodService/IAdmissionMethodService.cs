using CapstoneAPI.DataSets.AdmissionMethod;
using CapstoneAPI.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Services.AdmissionMethodService
{
    public interface IAdmissionMethodService
    {
        Task<Response<IEnumerable<AdmissionMethodDataSet>>> GetAllAdmsstionMethods();
    }
}
