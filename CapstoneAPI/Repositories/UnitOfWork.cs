﻿using CapstoneAPI.Models;
using CapstoneAPI.Repositories.Rank;
using System;
using System.Threading.Tasks;

namespace CapstoneAPI.Repositories
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly CapstoneDBContext _context;
        private bool disposed = false;
        private IGenericRepository<SubjectGroup> _subjectGroupRepository;
        private IGenericRepository<SubjectGroupDetail> _subjectGroupDetailRepository;
        private IGenericRepository<WeightNumber> _weightNumberRepository;
        private IGenericRepository<Major> _majorRepository;
        private IGenericRepository<Subject> _subjectRepository;
        private IGenericRepository<University> _universityRepository;
        private IGenericRepository<EntryMark> _entryMarkRepository;
        private IGenericRepository<Test> _testRepository;
        private IGenericRepository<Option> _optionRepository;
        private IGenericRepository<TestSubmission> _testSubmissionRepository;
        private IGenericRepository<Question> _questionRepository;
        private IGenericRepository<User> _userRepository;
        private IGenericRepository<Role> _roleRepository;
        private IGenericRepository<MajorDetail> _majorDetailRepository;
        private IGenericRepository<Article> _articleRepository;
        private IGenericRepository<QuestionSubmisstion> _questionSubmisstionRepository;
        private IGenericRepository<TrainingProgram> _trainingProgramRepository;
        private IGenericRepository<AdmissionCriterion> _admissionCriterionRepository;
        private IGenericRepository<UserMajorDetail> _userMajorDetailRepository;
        private IGenericRepository<Transcript> _transcriptRepository;
        private IGenericRepository<UniversityArticle> _universityArticleRepository;
        private IGenericRepository<MajorArticle> _majorArticleRepository;
        private IRankRepository _rankRepository;

        public IGenericRepository<SubjectGroup> SubjectGroupRepository
        {
            get { return _subjectGroupRepository ??= new GenericRepository<SubjectGroup>(_context); }
        }
        public IGenericRepository<University> UniversityRepository
        {
            get { return _universityRepository ??= new GenericRepository<University>(_context); }
        }
        public IGenericRepository<Major> MajorRepository
        {
            get { return _majorRepository ??= new GenericRepository<Major>(_context); }
        }

        public IGenericRepository<SubjectGroupDetail> SubjecGroupDetailRepository
        {
            get { return _subjectGroupDetailRepository ??= new GenericRepository<SubjectGroupDetail>(_context); }
        }

        public IGenericRepository<WeightNumber> WeightNumberRepository
        {
            get { return _weightNumberRepository ??= new GenericRepository<WeightNumber>(_context); }
        }

        public IGenericRepository<Subject> SubjectRepository
        {
            get { return _subjectRepository ??= new GenericRepository<Subject>(_context); }
        }

        public IGenericRepository<EntryMark> EntryMarkRepository
        {
            get { return _entryMarkRepository ??= new GenericRepository<EntryMark>(_context); }
        }

        public IGenericRepository<Test> TestRepository
        {
            get { return _testRepository ??= new GenericRepository<Test>(_context); }
        }

        public IGenericRepository<Option> OptionRepository
        {
            get { return _optionRepository ??= new GenericRepository<Option>(_context); }
        }

        public IGenericRepository<TestSubmission> TestSubmissionRepository
        {
            get { return _testSubmissionRepository ??= new GenericRepository<TestSubmission>(_context); }
        }
        public IGenericRepository<Question> QuestionRepository
        {
            get { return _questionRepository ??= new GenericRepository<Question>(_context); }
        }

        public IGenericRepository<User> UserRepository
        {
            get { return _userRepository ??= new GenericRepository<User>(_context); }
        }
        public IGenericRepository<Role> RoleRepository
        {
            get { return _roleRepository ??= new GenericRepository<Role>(_context); }
        }

        public IGenericRepository<MajorDetail> MajorDetailRepository
        {
            get { return _majorDetailRepository ??= new GenericRepository<MajorDetail>(_context); }
        }

        public IGenericRepository<Article> ArticleRepository
        {
            get { return _articleRepository ??= new GenericRepository<Article>(_context); }
        }

        public IGenericRepository<QuestionSubmisstion> QuestionSubmisstionRepository
        {
            get { return _questionSubmisstionRepository ??= new GenericRepository<QuestionSubmisstion>(_context); }
        }

        public IGenericRepository<TrainingProgram> TrainingProgramRepository
        {
            get { return _trainingProgramRepository ??= new GenericRepository<TrainingProgram>(_context); }
        }

        public IGenericRepository<AdmissionCriterion> AdmissionCriterionRepository
        {
            get { return _admissionCriterionRepository ??= new GenericRepository<AdmissionCriterion>(_context); }
        }
        public IGenericRepository<UserMajorDetail> UserMajorDetailRepository
        {
            get { return _userMajorDetailRepository ??= new GenericRepository<UserMajorDetail>(_context); }
        }
        public IGenericRepository<Transcript> TranscriptRepository
        {
            get { return _transcriptRepository ??= new GenericRepository<Transcript>(_context); }
        }
        public IGenericRepository<UniversityArticle> UniversityArticleRepository
        {
            get { return _universityArticleRepository ??= new GenericRepository<UniversityArticle>(_context); }
        }
        public IGenericRepository<MajorArticle> MajorArticleRepository
        {
            get { return _majorArticleRepository ??= new GenericRepository<MajorArticle>(_context); }
        }
        public IRankRepository RankRepository
        {
            get { return _rankRepository ??= new RankRepository(_context); }
        }

        public UnitOfWork(CapstoneDBContext context)
        {
            _context = context;
        }

        public async Task<int> CommitAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            Dispose(true);
            System.GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            this.disposed = true;
        }
    }
}
