using AutoMapper;
using CapstoneAPI.DataSets.Major;
using CapstoneAPI.DataSets.Option;
using CapstoneAPI.DataSets.Question;
using CapstoneAPI.DataSets.Subject;
using CapstoneAPI.DataSets.Test;
using CapstoneAPI.DataSets.University;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.DataSets
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Models.Major, MajorDataSet>();
            CreateMap<Models.Subject, SubjectDataSet>();
            CreateMap<Models.University, UniversityDataSet>();
            CreateMap<Models.Test, TestDataSet>();
            CreateMap<Models.Question, QuestionDataSet>();
            CreateMap<Models.Option, OptionDataSet>();
        }
    }
}
