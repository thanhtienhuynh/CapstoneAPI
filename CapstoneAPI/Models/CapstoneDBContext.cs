using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace CapstoneAPI.Models
{
    public partial class CapstoneDBContext : DbContext
    {
        public CapstoneDBContext()
        {
        }

        public CapstoneDBContext(DbContextOptions<CapstoneDBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<AdmissionCriterion> AdmissionCriteria { get; set; }
        public virtual DbSet<AdmissionMethod> AdmissionMethods { get; set; }
        public virtual DbSet<Article> Articles { get; set; }
        public virtual DbSet<Career> Careers { get; set; }
        public virtual DbSet<EntryMark> EntryMarks { get; set; }
        public virtual DbSet<FollowingDetail> FollowingDetails { get; set; }
        public virtual DbSet<Major> Majors { get; set; }
        public virtual DbSet<MajorArticle> MajorArticles { get; set; }
        public virtual DbSet<MajorCareer> MajorCareers { get; set; }
        public virtual DbSet<MajorDetail> MajorDetails { get; set; }
        public virtual DbSet<MajorSubjectGroup> MajorSubjectGroups { get; set; }
        public virtual DbSet<Option> Options { get; set; }
        public virtual DbSet<Province> Provinces { get; set; }
        public virtual DbSet<Question> Questions { get; set; }
        public virtual DbSet<QuestionSubmisstion> QuestionSubmisstions { get; set; }
        public virtual DbSet<Rank> Ranks { get; set; }
        public virtual DbSet<Region> Regions { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<Season> Seasons { get; set; }
        public virtual DbSet<SpecialSubjectGroup> SpecialSubjectGroups { get; set; }
        public virtual DbSet<SubAdmissionCriterion> SubAdmissionCriteria { get; set; }
        public virtual DbSet<Subject> Subjects { get; set; }
        public virtual DbSet<SubjectGroup> SubjectGroups { get; set; }
        public virtual DbSet<SubjectGroupDetail> SubjectGroupDetails { get; set; }
        public virtual DbSet<SubjectWeight> SubjectWeights { get; set; }
        public virtual DbSet<Test> Tests { get; set; }
        public virtual DbSet<TestSubmission> TestSubmissions { get; set; }
        public virtual DbSet<TestType> TestTypes { get; set; }
        public virtual DbSet<TrainingProgram> TrainingPrograms { get; set; }
        public virtual DbSet<Transcript> Transcripts { get; set; }
        public virtual DbSet<TranscriptType> TranscriptTypes { get; set; }
        public virtual DbSet<University> Universities { get; set; }
        public virtual DbSet<UniversityArticle> UniversityArticles { get; set; }
        public virtual DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<AdmissionCriterion>(entity =>
            {
                entity.HasKey(e => e.MajorDetailId);

                entity.ToTable("AdmissionCriterion");

                entity.Property(e => e.MajorDetailId)
                    .ValueGeneratedNever()
                    .HasColumnName("MajorDetail_Id");

                entity.HasOne(d => d.MajorDetail)
                    .WithOne(p => p.AdmissionCriterion)
                    .HasForeignKey<AdmissionCriterion>(d => d.MajorDetailId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AdmissionCriterion_MajorDetail");
            });

            modelBuilder.Entity<AdmissionMethod>(entity =>
            {
                entity.ToTable("AdmissionMethod");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<Article>(entity =>
            {
                entity.ToTable("Article");

                entity.Property(e => e.CrawlerDate).HasColumnType("datetime");

                entity.Property(e => e.PostedDate).HasColumnType("datetime");

                entity.Property(e => e.PublicFromDate).HasColumnType("datetime");

                entity.Property(e => e.PublicToDate).HasColumnType("datetime");

                entity.Property(e => e.PublishedPage).HasMaxLength(100);

                entity.Property(e => e.Title).HasMaxLength(200);
            });

            modelBuilder.Entity<Career>(entity =>
            {
                entity.ToTable("Career");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200);
            });

            modelBuilder.Entity<EntryMark>(entity =>
            {
                entity.ToTable("EntryMark");

                entity.Property(e => e.MajorSubjectGroupId).HasColumnName("MajorSubjectGroup_Id");

                entity.Property(e => e.SubAdmissionCriterionId).HasColumnName("SubAdmissionCriterion_Id");

                entity.HasOne(d => d.MajorSubjectGroup)
                    .WithMany(p => p.EntryMarks)
                    .HasForeignKey(d => d.MajorSubjectGroupId)
                    .HasConstraintName("FK_EntryMark_MajorSubjectGroup");

                entity.HasOne(d => d.SubAdmissionCriterion)
                    .WithMany(p => p.EntryMarks)
                    .HasForeignKey(d => d.SubAdmissionCriterionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_EntryMark_SubAdmissionCriterion");
            });

            modelBuilder.Entity<FollowingDetail>(entity =>
            {
                entity.ToTable("FollowingDetail");

                entity.Property(e => e.EntryMarkId).HasColumnName("EntryMark_Id");

                entity.Property(e => e.UserId).HasColumnName("User_Id");

                entity.HasOne(d => d.EntryMark)
                    .WithMany(p => p.FollowingDetails)
                    .HasForeignKey(d => d.EntryMarkId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_FollowingDetail_EntryMark");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.FollowingDetails)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_FollowingDetail_User");
            });

            modelBuilder.Entity<Major>(entity =>
            {
                entity.ToTable("Major");

                entity.Property(e => e.Code)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200);
            });

            modelBuilder.Entity<MajorArticle>(entity =>
            {
                entity.ToTable("MajorArticle");

                entity.Property(e => e.ArticleId).HasColumnName("Article_Id");

                entity.Property(e => e.MajorId).HasColumnName("Major_Id");

                entity.HasOne(d => d.Article)
                    .WithMany(p => p.MajorArticles)
                    .HasForeignKey(d => d.ArticleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_MajorArticle_Article");

                entity.HasOne(d => d.Major)
                    .WithMany(p => p.MajorArticles)
                    .HasForeignKey(d => d.MajorId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_MajorArticle_Major");
            });

            modelBuilder.Entity<MajorCareer>(entity =>
            {
                entity.ToTable("MajorCareer");

                entity.Property(e => e.CareerId).HasColumnName("Career_Id");

                entity.Property(e => e.MajorId).HasColumnName("Major_Id");

                entity.HasOne(d => d.Career)
                    .WithMany(p => p.MajorCareers)
                    .HasForeignKey(d => d.CareerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_MajorCareer_Career");

                entity.HasOne(d => d.Major)
                    .WithMany(p => p.MajorCareers)
                    .HasForeignKey(d => d.MajorId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_MajorCareer_Major");
            });

            modelBuilder.Entity<MajorDetail>(entity =>
            {
                entity.ToTable("MajorDetail");

                entity.Property(e => e.MajorCode)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("Major_Code");

                entity.Property(e => e.MajorId).HasColumnName("Major_Id");

                entity.Property(e => e.SeasonId).HasColumnName("Season_Id");

                entity.Property(e => e.TrainingProgramId).HasColumnName("Training_Program_Id");

                entity.Property(e => e.UniversityId).HasColumnName("University_Id");

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.HasOne(d => d.Major)
                    .WithMany(p => p.MajorDetails)
                    .HasForeignKey(d => d.MajorId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_MajorDetail_Major");

                entity.HasOne(d => d.Season)
                    .WithMany(p => p.MajorDetails)
                    .HasForeignKey(d => d.SeasonId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_MajorDetail_Season");

                entity.HasOne(d => d.TrainingProgram)
                    .WithMany(p => p.MajorDetails)
                    .HasForeignKey(d => d.TrainingProgramId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_MajorDetail_TrainingProgram");

                entity.HasOne(d => d.University)
                    .WithMany(p => p.MajorDetails)
                    .HasForeignKey(d => d.UniversityId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_MajorDetail_University");
            });

            modelBuilder.Entity<MajorSubjectGroup>(entity =>
            {
                entity.ToTable("MajorSubjectGroup");

                entity.Property(e => e.MajorId).HasColumnName("Major_Id");

                entity.Property(e => e.SubjectGroupId).HasColumnName("SubjectGroup_Id");

                entity.HasOne(d => d.Major)
                    .WithMany(p => p.MajorSubjectGroups)
                    .HasForeignKey(d => d.MajorId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_MajorSubjectGroup_Major");

                entity.HasOne(d => d.SubjectGroup)
                    .WithMany(p => p.MajorSubjectGroups)
                    .HasForeignKey(d => d.SubjectGroupId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_MajorSubjectGroup_SubjectGroup");
            });

            modelBuilder.Entity<Option>(entity =>
            {
                entity.ToTable("Option");

                entity.Property(e => e.Content).IsRequired();

                entity.Property(e => e.QuestionId).HasColumnName("Question_Id");

                entity.HasOne(d => d.Question)
                    .WithMany(p => p.Options)
                    .HasForeignKey(d => d.QuestionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Option_Question");
            });

            modelBuilder.Entity<Province>(entity =>
            {
                entity.ToTable("Province");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.RegionId).HasColumnName("Region_Id");

                entity.HasOne(d => d.Region)
                    .WithMany(p => p.Provinces)
                    .HasForeignKey(d => d.RegionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Province_Region");
            });

            modelBuilder.Entity<Question>(entity =>
            {
                entity.ToTable("Question");

                entity.Property(e => e.Content).IsRequired();

                entity.Property(e => e.Result)
                    .IsRequired()
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.TestId).HasColumnName("Test_Id");

                entity.HasOne(d => d.Test)
                    .WithMany(p => p.Questions)
                    .HasForeignKey(d => d.TestId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Question_Test");
            });

            modelBuilder.Entity<QuestionSubmisstion>(entity =>
            {
                entity.ToTable("QuestionSubmisstion");

                entity.Property(e => e.QuestionId).HasColumnName("Question_Id");

                entity.Property(e => e.Result)
                    .IsRequired()
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.TestSubmissionId).HasColumnName("TestSubmission_Id");

                entity.HasOne(d => d.Question)
                    .WithMany(p => p.QuestionSubmisstions)
                    .HasForeignKey(d => d.QuestionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_QuestionSubmisstion_Question");

                entity.HasOne(d => d.TestSubmission)
                    .WithMany(p => p.QuestionSubmisstions)
                    .HasForeignKey(d => d.TestSubmissionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_QuestionSubmisstion_TestSubmission");
            });

            modelBuilder.Entity<Rank>(entity =>
            {
                entity.HasKey(e => e.FollowingDetailId)
                    .HasName("PK_Rank_1");

                entity.ToTable("Rank");

                entity.Property(e => e.FollowingDetailId)
                    .ValueGeneratedNever()
                    .HasColumnName("FollowingDetail_Id");

                entity.Property(e => e.TranscriptTypeId).HasColumnName("TranscriptType_Id");

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.HasOne(d => d.FollowingDetail)
                    .WithOne(p => p.Rank)
                    .HasForeignKey<Rank>(d => d.FollowingDetailId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Rank_FollowingDetail");

                entity.HasOne(d => d.TranscriptType)
                    .WithMany(p => p.Ranks)
                    .HasForeignKey(d => d.TranscriptTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Rank_TranscriptType");
            });

            modelBuilder.Entity<Region>(entity =>
            {
                entity.ToTable("Region");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Role");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<Season>(entity =>
            {
                entity.ToTable("Season");

                entity.Property(e => e.FromDate).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.ToDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<SpecialSubjectGroup>(entity =>
            {
                entity.ToTable("SpecialSubjectGroup");

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<SubAdmissionCriterion>(entity =>
            {
                entity.ToTable("SubAdmissionCriterion");

                entity.Property(e => e.AdmissionCriterionId).HasColumnName("AdmissionCriterion_Id");

                entity.Property(e => e.AdmissionMethodId).HasColumnName("AdmissionMethod_Id");

                entity.Property(e => e.ProvinceId).HasColumnName("Province_Id");

                entity.HasOne(d => d.AdmissionCriterion)
                    .WithMany(p => p.SubAdmissionCriteria)
                    .HasForeignKey(d => d.AdmissionCriterionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SubAdmissionCriterion_AdmissionCriterion");

                entity.HasOne(d => d.AdmissionMethod)
                    .WithMany(p => p.SubAdmissionCriteria)
                    .HasForeignKey(d => d.AdmissionMethodId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SubAdmissionCriterion_AdmissionMethod");

                entity.HasOne(d => d.Province)
                    .WithMany(p => p.SubAdmissionCriteria)
                    .HasForeignKey(d => d.ProvinceId)
                    .HasConstraintName("FK_SubAdmissionCriterion_Province");
            });

            modelBuilder.Entity<Subject>(entity =>
            {
                entity.ToTable("Subject");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.SpecialSubjectGroupId).HasColumnName("SpecialSubjectGroup_Id");

                entity.HasOne(d => d.SpecialSubjectGroup)
                    .WithMany(p => p.Subjects)
                    .HasForeignKey(d => d.SpecialSubjectGroupId)
                    .HasConstraintName("FK_Subject_SpecialSubjectGroup");
            });

            modelBuilder.Entity<SubjectGroup>(entity =>
            {
                entity.ToTable("SubjectGroup");

                entity.Property(e => e.GroupCode)
                    .IsRequired()
                    .HasMaxLength(10)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<SubjectGroupDetail>(entity =>
            {
                entity.ToTable("SubjectGroupDetail");

                entity.Property(e => e.SpecialSubjectGroupId).HasColumnName("SpecialSubjectGroup_Id");

                entity.Property(e => e.SubjectGroupId).HasColumnName("SubjectGroup_Id");

                entity.Property(e => e.SubjectId).HasColumnName("Subject_Id");

                entity.HasOne(d => d.SpecialSubjectGroup)
                    .WithMany(p => p.SubjectGroupDetails)
                    .HasForeignKey(d => d.SpecialSubjectGroupId)
                    .HasConstraintName("FK_GroupDetail_SpecialSubjectGroup");

                entity.HasOne(d => d.SubjectGroup)
                    .WithMany(p => p.SubjectGroupDetails)
                    .HasForeignKey(d => d.SubjectGroupId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_GroupDetail_SubjectGroup");

                entity.HasOne(d => d.Subject)
                    .WithMany(p => p.SubjectGroupDetails)
                    .HasForeignKey(d => d.SubjectId)
                    .HasConstraintName("FK_GroupDetail_Subject");
            });

            modelBuilder.Entity<SubjectWeight>(entity =>
            {
                entity.ToTable("SubjectWeight");

                entity.Property(e => e.MajorSubjectGroupId).HasColumnName("MajorSubjectGroup_Id");

                entity.Property(e => e.SubjectGroupDetailId).HasColumnName("SubjectGroupDetail_Id");

                entity.HasOne(d => d.MajorSubjectGroup)
                    .WithMany(p => p.SubjectWeights)
                    .HasForeignKey(d => d.MajorSubjectGroupId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SubjectWeight_MajorSubjectGroup");

                entity.HasOne(d => d.SubjectGroupDetail)
                    .WithMany(p => p.SubjectWeights)
                    .HasForeignKey(d => d.SubjectGroupDetailId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_GroupWeight_GroupDetail");
            });

            modelBuilder.Entity<Test>(entity =>
            {
                entity.ToTable("Test");

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.SubjectId).HasColumnName("Subject_Id");

                entity.Property(e => e.TestTypeId).HasColumnName("TestType_Id");

                entity.Property(e => e.UniversityId).HasColumnName("University_Id");

                entity.Property(e => e.UserId).HasColumnName("User_Id");

                entity.HasOne(d => d.Subject)
                    .WithMany(p => p.Tests)
                    .HasForeignKey(d => d.SubjectId)
                    .HasConstraintName("FK_Test_Subject1");

                entity.HasOne(d => d.TestType)
                    .WithMany(p => p.Tests)
                    .HasForeignKey(d => d.TestTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Test_TestType");

                entity.HasOne(d => d.University)
                    .WithMany(p => p.Tests)
                    .HasForeignKey(d => d.UniversityId)
                    .HasConstraintName("FK_Test_University");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Tests)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Test_User");
            });

            modelBuilder.Entity<TestSubmission>(entity =>
            {
                entity.ToTable("TestSubmission");

                entity.Property(e => e.SubmissionDate).HasColumnType("datetime");

                entity.Property(e => e.TestId).HasColumnName("Test_Id");

                entity.Property(e => e.UserId).HasColumnName("User_Id");

                entity.HasOne(d => d.Test)
                    .WithMany(p => p.TestSubmissions)
                    .HasForeignKey(d => d.TestId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TestSubmission_Test");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.TestSubmissions)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TestSubmission_User");
            });

            modelBuilder.Entity<TestType>(entity =>
            {
                entity.ToTable("TestType");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<TrainingProgram>(entity =>
            {
                entity.ToTable("TrainingProgram");

                entity.Property(e => e.Name).IsRequired();
            });

            modelBuilder.Entity<Transcript>(entity =>
            {
                entity.ToTable("Transcript");

                entity.Property(e => e.DateRecord).HasColumnType("datetime");

                entity.Property(e => e.SubjectId).HasColumnName("Subject_Id");

                entity.Property(e => e.TranscriptTypeId).HasColumnName("TranscriptType_Id");

                entity.Property(e => e.UserId).HasColumnName("User_Id");

                entity.HasOne(d => d.Subject)
                    .WithMany(p => p.Transcripts)
                    .HasForeignKey(d => d.SubjectId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Transcript_Subject");

                entity.HasOne(d => d.TranscriptType)
                    .WithMany(p => p.Transcripts)
                    .HasForeignKey(d => d.TranscriptTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Transcript_TranscriptType");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Transcripts)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Transcript_User");
            });

            modelBuilder.Entity<TranscriptType>(entity =>
            {
                entity.ToTable("TranscriptType");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<University>(entity =>
            {
                entity.ToTable("University");

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Latitude).HasColumnType("decimal(12, 9)");

                entity.Property(e => e.Longitude).HasColumnType("decimal(12, 9)");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Phone).HasMaxLength(150);

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.Property(e => e.WebUrl).HasColumnName("WebURL");
            });

            modelBuilder.Entity<UniversityArticle>(entity =>
            {
                entity.ToTable("UniversityArticle");

                entity.Property(e => e.ArticleId).HasColumnName("Article_Id");

                entity.Property(e => e.UniversityId).HasColumnName("University_Id");

                entity.HasOne(d => d.Article)
                    .WithMany(p => p.UniversityArticles)
                    .HasForeignKey(d => d.ArticleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UniversityArticle_Article");

                entity.HasOne(d => d.University)
                    .WithMany(p => p.UniversityArticles)
                    .HasForeignKey(d => d.UniversityId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UniversityArticle_University");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("User");

                entity.Property(e => e.Email)
                    .HasMaxLength(320)
                    .IsUnicode(false);

                entity.Property(e => e.Fullname).HasMaxLength(50);

                entity.Property(e => e.Password).IsUnicode(false);

                entity.Property(e => e.Phone)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.ProvinceId).HasColumnName("Province_Id");

                entity.Property(e => e.RoleId).HasColumnName("Role_Id");

                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.Province)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.ProvinceId)
                    .HasConstraintName("FK_User_Province");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_User_Role");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
