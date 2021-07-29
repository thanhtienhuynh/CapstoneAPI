using CapstoneAPI.Features.AdmissionMethod.DataSet;
using CapstoneAPI.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.AdmissionMethod.Service
{
    public interface IAdmissionMethodService
    {
        Task<Response<IEnumerable<AdmissionMethodDataSet>>> GetAllAdmsstionMethods();
    }
}
