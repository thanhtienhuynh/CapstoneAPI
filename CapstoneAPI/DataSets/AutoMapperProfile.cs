using AutoMapper;
using CapstoneAPI.DataSets.AdmissionMethod;
using CapstoneAPI.DataSets.Article;
using CapstoneAPI.DataSets.FollowingDetail;
using CapstoneAPI.DataSets.Major;
using CapstoneAPI.DataSets.Option;
using CapstoneAPI.DataSets.Province;
using CapstoneAPI.DataSets.Question;
using CapstoneAPI.DataSets.QuestionSubmission;
using CapstoneAPI.DataSets.Rank;
using CapstoneAPI.DataSets.Season;
using CapstoneAPI.DataSets.SpecialSubjectGroup;
using CapstoneAPI.DataSets.Subject;
using CapstoneAPI.DataSets.SubjectGroup;
using CapstoneAPI.DataSets.Test;
using CapstoneAPI.DataSets.TestSubmission;
using CapstoneAPI.DataSets.TrainingProgram;
using CapstoneAPI.DataSets.University;
using CapstoneAPI.DataSets.User;

namespace CapstoneAPI.DataSets
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Models.Major, MajorDataSet>();
            CreateMap<Models.Major, AdminMajorDataSet>();
            CreateMap<Models.Subject, SubjectDataSet>();
            CreateMap<Models.University, TrainingProgramBasedUniversityDataSet>();
            CreateMap<CreateUniversityDataset, Models.University>();
            CreateMap<Models.University, CreateUniversityDataset>();
            CreateMap<Models.University, UniversityGroupByTrainingProgramDataSet>();
            CreateMap<Models.University, FollowingDetailGroupByUniversityDataSet>();
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
            CreateMap<Models.Major, FollowingDetailGroupByMajorDataSet>();
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
            //CreateMap<Models.UserMajorDetail, UserMajorDetailDataSet>();
            CreateMap<Models.FollowingDetail, FollowingDetailDataSet>();
            CreateMap<Models.Article, AdminArticleDetailDataSet>();
            CreateMap<Models.Article, ApprovingArticleDataSet>();
            CreateMap<Models.University, ApprovingArticleUniversityResponse>();
            CreateMap<Models.Province, ProvinceDataSet>();
            CreateMap<Models.SpecialSubjectGroup, SpecialSubjectGroupDataSet>();
            CreateMap<Models.AdmissionMethod, AdmissionMethodDataSet>();
            CreateMap<Models.Season, AdminSeasonDataSet>();
            CreateMap<AdminSeasonDataSet, Models.Season>();
        }
    }
}
