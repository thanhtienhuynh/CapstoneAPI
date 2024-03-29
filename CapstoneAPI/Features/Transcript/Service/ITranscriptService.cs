﻿using CapstoneAPI.Features.SubjectGroup.DataSet;
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
        Task<Response<bool>> SaveMarkOfUser(string token, SubjectGroupParam subjectGroupParam);
        Task<Response<bool>> SaveSingleTranscript(string token, TranscriptParam transcriptParam);
    }
}
