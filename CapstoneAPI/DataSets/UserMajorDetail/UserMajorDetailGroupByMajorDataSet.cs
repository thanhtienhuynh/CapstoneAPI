﻿using CapstoneAPI.DataSets.University;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.DataSets.UserMajorDetail
{
    public class UserMajorDetailGroupByMajorDataSet
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public IEnumerable<TrainingProgramGroupByMajorDataSet> TrainingProgramGroupByMajorDataSets { get; set; }
    }

    public class TrainingProgramGroupByMajorDataSet
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<UniversityGroupByTrainingProgramDataSet> UniversityGroupByTrainingProgramDataSets { get; set; }
    }

    public class UniversityGroupByTrainingProgramDataSet
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string LogoUrl { get; set; }
        public string Description { get; set; }
        public string Phone { get; set; }
        public string WebUrl { get; set; }
        public int? TuitionType { get; set; }
        public int? TuitionFrom { get; set; }
        public int? TuitionTo { get; set; }
        public int? Rating { get; set; }
        public string MajorCode { get; set; }
        public double? NewestEntryMark { get; set; }
        public int? NumberOfStudent { get; set; }
        public int YearOfEntryMark { get; set; }
        public int? PositionOfUser { get; set; }
        public int TotalUserCared { get; set; }
        public int SubjectGroupId { get; set; }
        public string SubjectGroupCode { get; set; }
        public double? RankingMark { get; set; }
    }
}