using AutoMapper;
using CapstoneAPI.DataSets.Option;
using CapstoneAPI.DataSets.Question;
using CapstoneAPI.DataSets.QuestionSubmission;
using CapstoneAPI.DataSets.SpecialSubjectGroup;
using CapstoneAPI.Features.AdmissionMethod.DataSet;
using CapstoneAPI.Features.Article.DataSet;
using CapstoneAPI.Features.FollowingDetail.DataSet;
using CapstoneAPI.Features.Major.DataSet;
using CapstoneAPI.Features.Notification.DataSet;
using CapstoneAPI.Features.Province.DataSet;
using CapstoneAPI.Features.Rank.DataSet;
using CapstoneAPI.Features.Season.DataSet;
using CapstoneAPI.Features.Subject.DataSet;
using CapstoneAPI.Features.SubjectGroup.DataSet;
using CapstoneAPI.Features.Test.DataSet;
using CapstoneAPI.Features.TestSubmission.DataSet;
using CapstoneAPI.Features.TestType.DataSet;
using CapstoneAPI.Features.TrainingProgram.DataSet;
using CapstoneAPI.Features.University.DataSet;
using CapstoneAPI.Features.User.DataSet;

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
            CreateMap<Models.FollowingDetail, FollowingDetailDataSet>();
            CreateMap<Models.FollowingDetail, RankFollowingDetailDataSet>();
            CreateMap<Models.Article, AdminArticleDetailDataSet>();
            CreateMap<Models.Article, ApprovingArticleDataSet>();
            CreateMap<Models.University, ApprovingArticleUniversityResponse>();
            CreateMap<NewQuestionParam, Models.Question>();
            CreateMap<NewTestParam, Models.Test>();
            CreateMap<NewOptionParam, Models.Option>();

            CreateMap<Models.Province, ProvinceDataSet>();
            CreateMap<Models.SpecialSubjectGroup, SpecialSubjectGroupDataSet>();
            CreateMap<Models.AdmissionMethod, AdmissionMethodDataSet>();
            CreateMap<Models.Season, AdminSeasonDataSet>();

            CreateMap<Models.Major, MajorSubjectWeightDataSet>();
            CreateMap<CreateMajorSubjectWeightDataSet, Models.Major>();




            CreateMap<Models.Test, TestPagingDataSet>();
            CreateMap<Models.TestType, TestTypeDataSet>();
            CreateMap<Models.Major, NumberUniversityInMajorDataSet>();
            CreateMap<Models.Major, MajorDetailDataSet>();

            CreateMap<Models.Career, CareerDataSet>();
            CreateMap<Models.Test, TestAdminDataSet>();

            CreateMap<Models.Notification, NotificationDataSet>();
        }
    }
}
