﻿using AutoMapper;
using CapstoneAPI.DataSets.Article;
using CapstoneAPI.DataSets.Major;
using CapstoneAPI.DataSets.Option;
using CapstoneAPI.DataSets.Question;
using CapstoneAPI.DataSets.QuestionSubmission;
using CapstoneAPI.DataSets.Rank;
using CapstoneAPI.DataSets.Subject;
using CapstoneAPI.DataSets.SubjectGroup;
using CapstoneAPI.DataSets.Test;
using CapstoneAPI.DataSets.TestSubmission;
using CapstoneAPI.DataSets.TrainingProgram;
using CapstoneAPI.DataSets.University;
using CapstoneAPI.DataSets.User;
using CapstoneAPI.DataSets.UserMajorDetail;
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
            CreateMap<Models.Major, AdminMajorDataSet>();
            CreateMap<Models.Subject, SubjectDataSet>();
            CreateMap<Models.University, UniversityDataSet>();
            CreateMap<CreateUniversityDataset, Models.University>();
            CreateMap<Models.University, CreateUniversityDataset>();
            CreateMap<Models.University, UniversityGroupByTrainingProgramDataSet>();
            CreateMap<Models.University, UserMajorDetailGroupByUniversityDataSet>();
            CreateMap<Models.Test, TestDataSet>();
            CreateMap<Models.Question, QuestionDataSet>();
            CreateMap<Models.Option, OptionDataSet>();
            CreateMap<Models.TestSubmission, TestSubmissionDataSet>();
            CreateMap<Models.TestSubmission, DetailTestSubmissionDataSet>();
            CreateMap<Models.TestSubmission, UserTestSubmissionDataSet>();
            CreateMap<Models.User, UserDataSet>();
            CreateMap<Models.User, RankingUserInformation>();
            CreateMap<Models.University, DetailUniversityDataSet>();
            CreateMap<Models.University, AdminUniversityDataSet>();
            CreateMap<Models.Major, UniMajorDataSet>();
            CreateMap<Models.Major, UserMajorDetailGroupByMajorDataSet>();
            CreateMap<Models.SubjectGroup, UniSubjectGroupDataSet>();
            CreateMap<Models.SubjectGroup, AdminSubjectGroupDataSet>();
            CreateMap<Models.EntryMark, UniEntryMarkDataSet>();
            CreateMap<CreateMajorDataSet, Models.Major>();
            CreateMap<Models.Major, ResultOfCreateMajorDataSet>();
            CreateMap<Models.QuestionSubmisstion, QuestionSubmissionDataSet>();
            CreateMap<Models.TrainingProgram, AdminTrainingProgramDataSet>();
            CreateMap<Models.TrainingProgram, TrainingProgramGroupByMajorDataSet>();
            CreateMap<Models.TrainingProgram, TrainingProgramGroupByUniversityDataSet>();
            CreateMap<Models.Article, ArticleCollapseDataSet>();
            CreateMap<Models.Article, AdminArticleCollapseDataSet>();
            CreateMap<Models.Article, ArticleDetailDataSet>();
            CreateMap<Models.Rank, RankDataSet>();
            CreateMap<Models.UserMajorDetail, UserMajorDetailDataSet>();
            CreateMap<Models.Article, AdminArticleDetailDataSet>();
            CreateMap<Models.Article, ApprovingArticleDataSet>();
        }
    }
}
