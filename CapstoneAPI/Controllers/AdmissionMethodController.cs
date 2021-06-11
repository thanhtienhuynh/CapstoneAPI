using CapstoneAPI.DataSets.AdmissionMethod;
using CapstoneAPI.Services.AdmissionMethodService;
using CapstoneAPI.Wrappers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Controllers
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
        [HttpGet]
        public async Task<ActionResult<Response<IEnumerable<AdmissionMethodDataSet>>>> GetAdmissionMethods()
        {
            return Ok(await _service.GetAllAdmsstionMethods());
        }
    }
}
