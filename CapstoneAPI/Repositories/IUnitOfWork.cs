using CapstoneAPI.Models;
using CapstoneAPI.Repositories.Rank;
using CapstoneAPI.Repositories.Season;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Repositories
{
    public interface IUnitOfWork
    {
        IGenericRepository<SubjectGroup> SubjectGroupRepository { get; }
        IGenericRepository<SubjectWeight> SubjectWeightRepository { get; }
        IGenericRepository<SubjectGroupDetail> SubjecGroupDetailRepository { get; }
        IGenericRepository<Major> MajorRepository { get; }
        IGenericRepository<Subject> SubjectRepository { get; }
        IGenericRepository<University> UniversityRepository { get; }
        IGenericRepository<EntryMark> EntryMarkRepository { get; }
        IGenericRepository<Test> TestRepository { get; }
        IGenericRepository<Option> OptionRepository { get; }
        IGenericRepository<TestSubmission> TestSubmissionRepository { get; }
        IGenericRepository<Question> QuestionRepository { get; }
        IGenericRepository<User> UserRepository { get; }
        IGenericRepository<Role> RoleRepository { get; }
        IGenericRepository<MajorDetail> MajorDetailRepository { get; }
        IGenericRepository<Article> ArticleRepository { get; }
        IGenericRepository<QuestionSubmisstion> QuestionSubmisstionRepository { get; }
        IGenericRepository<TrainingProgram> TrainingProgramRepository { get; }
        IGenericRepository<AdmissionCriterion> AdmissionCriterionRepository { get; }
        IGenericRepository<FollowingDetail> FollowingDetailRepository { get; }
        IGenericRepository<Transcript> TranscriptRepository { get; }
        IGenericRepository<UniversityArticle> UniversityArticleRepository { get; }
        IGenericRepository<SubAdmissionCriterion> SubAdmissionCriterionRepository { get; }
        IGenericRepository<MajorSubjectGroup> MajorSubjectGroupRepository { get; }
        IGenericRepository<Province> ProvinceRepository { get; }
        ISeasonRepository SeasonRepository { get; }
        IGenericRepository<AdmissionMethod> AdmissionMethodRepository { get; }
        IGenericRepository<MajorArticle> MajorArticleRepository { get; }
        IRankRepository RankRepository { get; }
        IDbContextTransaction GetTransaction();
        Task<int> CommitAsync();
    }
}
