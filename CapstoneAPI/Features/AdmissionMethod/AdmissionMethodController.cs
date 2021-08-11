using CapstoneAPI.Features.AdmissionMethod.DataSet;
using CapstoneAPI.Features.AdmissionMethod.Service;
using CapstoneAPI.Helpers;
using CapstoneAPI.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.AdmissionMethod
{
    [Route("api/v1/admission-method")]
    [ApiController]
    public class AdmissionMethodController : ControllerBase
    {
        private readonly IAdmissionMethodService _service;
        public AdmissionMethodController(IAdmissionMethodService service)
        {
            _service = service;
        }

        [Authorize(Roles = Roles.Staff)]
        [HttpGet]
        public async Task<ActionResult<Response<IEnumerable<AdmissionMethodDataSet>>>> GetAdmissionMethods()
        {
            return Ok(await _service.GetAllAdmsstionMethods());
        }
    }
}
