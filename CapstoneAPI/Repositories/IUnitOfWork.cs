﻿using CapstoneAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Repositories
{
    public interface IUnitOfWork
    {
        IGenericRepository<SubjectGroup> SubjectGroupRepository { get; }
        IGenericRepository<WeightNumber> WeightNumberRepository { get; }
        IGenericRepository<SubjectGroupDetail> SubjecGroupDetailRepository { get; }
        IGenericRepository<Major> MajorRepository { get; }
        IGenericRepository<Subject> SubjectRepository { get; }
        IGenericRepository<University> UniversityRepository { get; }
        IGenericRepository<EntryMark> EntryMarkRepository { get; }
        IGenericRepository<Test>TestRepository { get; }
        IGenericRepository<Option> OptionRepository { get; }
        Task<int> CommitAsync();
    }
}
