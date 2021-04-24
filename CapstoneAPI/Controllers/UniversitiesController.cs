using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CapstoneAPI.Models;
using CapstoneAPI.DataSets.University;
using CapstoneAPI.Services.University;
using System.Net.Http.Headers;
using System.IO;
using Firebase.Auth;
using System.Threading;
using Firebase.Storage;
using CapstoneAPI.Helpers;

namespace CapstoneAPI.Controllers
{
    [Route("api/v1/university")]
    [ApiController]
    public class UniversitiesController : ControllerBase
    {
        private readonly IUniversityService _service;

        public UniversitiesController(IUniversityService service)
        {
            _service = service;
        }

        [HttpGet("suggestion")]
        public async Task<ActionResult<IEnumerable<UniversityDataSetBaseOnTrainingProgram>>> GetUniversityBySubjectGroupAndMajor([FromQuery] UniversityParam universityParam)
        {
            string token = Request.Headers["Authorization"];
            IEnumerable<UniversityDataSetBaseOnTrainingProgram> result = await _service.GetUniversityBySubjectGroupAndMajor(universityParam, token);
            if (result == null || !result.Any())
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpGet()]
        public async Task<ActionResult<IEnumerable<AdminUniversityDataSet>>> GetAllUniversities()
        {
            IEnumerable<AdminUniversityDataSet> result = await _service.GetUniversities();
            if (result == null || !result.Any())
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpGet("detail/{id}")]
        public async Task<ActionResult<DetailUniversityDataSet>> GetDetailUniversity([FromRoute] int id)
        {
            DetailUniversityDataSet result = await _service.GetDetailUniversity(id);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }
        [HttpPost]
        public async Task<ActionResult<AdminUniversityDataSet>> CreateAnUniversity([FromBody] CreateUniversityDataset createUniversityDataset)
        {
            AdminUniversityDataSet result = await _service.CreateNewAnUniversity(createUniversityDataset);
            if (result == null)
            {
                return BadRequest();
            }
            return Ok(result);
        }
        [HttpPut]
        public async Task<ActionResult<AdminUniversityDataSet>> UpdateUniversity([FromForm] AdminUniversityDataSet adminUniversityDataSet)
        {
            IFormFile logoImage = adminUniversityDataSet.files;
            if (logoImage != null)
            {
                if (Consts.ImageExtensions.Contains(Path.GetExtension(logoImage.FileName).ToUpperInvariant()))
                {

                    using (var ms = new MemoryStream())
                    {
                        logoImage.CopyTo(ms);
                        ms.Position = 0;
                        if (ms != null && ms.Length > 0)
                        {
                            var auth = new FirebaseAuthProvider(new FirebaseConfig(Consts.ApiKey));
                            var firebaseAuth = await auth.SignInWithEmailAndPasswordAsync(Consts.AuthEmail, Consts.AuthPassword);

                            // you can use CancellationTokenSource to cancel the upload midway
                            var cancellation = new CancellationTokenSource();

                            var task = new FirebaseStorage(
                                Consts.Bucket,
                                new FirebaseStorageOptions
                                {
                                    ThrowOnCancel = true, // when you cancel the upload, exception is thrown. By default no exception is thrown
                                    AuthTokenAsyncFactory = () => Task.FromResult(firebaseAuth.FirebaseToken),
                                })
                                .Child(Consts.LogoFolder)
                                .Child(adminUniversityDataSet.Code + Path.GetExtension(logoImage.FileName))
                                .PutAsync(ms, cancellation.Token);

                            adminUniversityDataSet.LogoUrl = await task;
                        }

                    }
                }
                else
                {
                    return BadRequest();
                }
            }
            AdminUniversityDataSet result = await _service.UpdateUniversity(adminUniversityDataSet);
            if (result == null)
            {
                return BadRequest();
            }
            return Ok(result);
        }
        [HttpPost("major-addition")]
        public async Task<ActionResult<DetailUniversityDataSet>> AddMajorToUniversity([FromBody] AddingMajorUniversityParam addingMajorUniversityParam)
        {
            DetailUniversityDataSet result = await _service.AddMajorToUniversity(addingMajorUniversityParam);
            if (result == null)
            {
                return BadRequest();
            }
            return Ok(result);
        }
        [HttpPut("major-updation")]
        public async Task<ActionResult<DetailUniversityDataSet>> UpdateMajorOfUniversity([FromBody] UpdatingMajorUniversityParam updatingMajorUniversityParam)
        {
            DetailUniversityDataSet result = await _service.UpdateMajorOfUniversity(updatingMajorUniversityParam);
            if (result == null)
            {
                return BadRequest();
            }
            return Ok(result);
        }

    }
}
