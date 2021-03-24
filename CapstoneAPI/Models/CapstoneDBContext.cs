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

        public virtual DbSet<Article> Articles { get; set; }
        public virtual DbSet<Career> Careers { get; set; }
        public virtual DbSet<EntryMark> EntryMarks { get; set; }
        public virtual DbSet<Major> Majors { get; set; }
        public virtual DbSet<MajorCareer> MajorCareers { get; set; }
        public virtual DbSet<MajorDetail> MajorDetails { get; set; }
        public virtual DbSet<Option> Options { get; set; }
        public virtual DbSet<Question> Questions { get; set; }
        public virtual DbSet<QuestionSubmisstion> QuestionSubmisstions { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<Subject> Subjects { get; set; }
        public virtual DbSet<SubjectGroup> SubjectGroups { get; set; }
        public virtual DbSet<SubjectGroupDetail> SubjectGroupDetails { get; set; }
        public virtual DbSet<Test> Tests { get; set; }
        public virtual DbSet<TestSubmission> TestSubmissions { get; set; }
        public virtual DbSet<TestType> TestTypes { get; set; }
        public virtual DbSet<Transcript> Transcripts { get; set; }
        public virtual DbSet<TranscriptType> TranscriptTypes { get; set; }
        public virtual DbSet<University> Universities { get; set; }
        public virtual DbSet<UniversityArticle> UniversityArticles { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserMajor> UserMajors { get; set; }
        public virtual DbSet<UserUniversity> UserUniversities { get; set; }
        public virtual DbSet<WeightNumber> WeightNumbers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

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

                entity.Property(e => e.Name).HasMaxLength(200);
            });

            modelBuilder.Entity<EntryMark>(entity =>
            {
                entity.ToTable("EntryMark");

                entity.Property(e => e.MajorDetailId).HasColumnName("MajorDetail_Id");

                entity.Property(e => e.SubjectGroupId).HasColumnName("SubjectGroup_Id");

                entity.HasOne(d => d.MajorDetail)
                    .WithMany(p => p.EntryMarks)
                    .HasForeignKey(d => d.MajorDetailId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_EntryMark_MajorDetail");

                entity.HasOne(d => d.SubjectGroup)
                    .WithMany(p => p.EntryMarks)
                    .HasForeignKey(d => d.SubjectGroupId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_EntryMark_SubjectGroup");
            });

            modelBuilder.Entity<Major>(entity =>
            {
                entity.ToTable("Major");

                entity.Property(e => e.Code)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Name).HasMaxLength(200);
            });

            modelBuilder.Entity<MajorCareer>(entity =>
            {
                entity.HasKey(e => new { e.MajorId, e.CareerId });

                entity.ToTable("MajorCareer");

                entity.Property(e => e.MajorId).HasColumnName("Major_Id");

                entity.Property(e => e.CareerId).HasColumnName("Career_Id");

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

                entity.Property(e => e.UniversityId).HasColumnName("University_Id");

                entity.HasOne(d => d.Major)
                    .WithMany(p => p.MajorDetails)
                    .HasForeignKey(d => d.MajorId)
                    .HasConstraintName("FK_Tution_Major");

                entity.HasOne(d => d.University)
                    .WithMany(p => p.MajorDetails)
                    .HasForeignKey(d => d.UniversityId)
                    .HasConstraintName("FK_Tution_University");
            });

            modelBuilder.Entity<Option>(entity =>
            {
                entity.ToTable("Option");

                entity.Property(e => e.OptionContent).IsRequired();

                entity.Property(e => e.QuestionId).HasColumnName("Question_Id");

                entity.HasOne(d => d.Question)
                    .WithMany(p => p.Options)
                    .HasForeignKey(d => d.QuestionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Option_Question");
            });

            modelBuilder.Entity<Question>(entity =>
            {
                entity.ToTable("Question");

                entity.Property(e => e.QuestionContent).IsRequired();

                entity.Property(e => e.Result)
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

            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Role");

                entity.Property(e => e.Name).HasMaxLength(50);
            });

            modelBuilder.Entity<Subject>(entity =>
            {
                entity.ToTable("Subject");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
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
                entity.HasKey(e => new { e.SubjectId, e.SubjectGroupId });

                entity.Property(e => e.SubjectId).HasColumnName("Subject_Id");

                entity.Property(e => e.SubjectGroupId).HasColumnName("SubjectGroup_Id");

                entity.HasOne(d => d.SubjectGroup)
                    .WithMany(p => p.SubjectGroupDetails)
                    .HasForeignKey(d => d.SubjectGroupId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SubjectGroupDetails_SubjectGroup");

                entity.HasOne(d => d.Subject)
                    .WithMany(p => p.SubjectGroupDetails)
                    .HasForeignKey(d => d.SubjectId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SubjectGroupDetails_Subject");
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

                entity.Property(e => e.Year).HasColumnType("date");

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
                    .HasConstraintName("FK_TestSubmission_Test");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.TestSubmissions)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_TestSubmission_User");
            });

            modelBuilder.Entity<TestType>(entity =>
            {
                entity.ToTable("TestType");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(50);
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

                entity.Property(e => e.Type)
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
                entity.HasKey(e => new { e.UniversityId, e.ArticleId })
                    .HasName("PK_University_AdmissionInformation");

                entity.ToTable("University_Article");

                entity.Property(e => e.UniversityId).HasColumnName("University_Id");

                entity.Property(e => e.ArticleId).HasColumnName("Article_Id");

                entity.HasOne(d => d.Article)
                    .WithMany(p => p.UniversityArticles)
                    .HasForeignKey(d => d.ArticleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_University_AdmissionInformation_AdmissionInformation");

                entity.HasOne(d => d.University)
                    .WithMany(p => p.UniversityArticles)
                    .HasForeignKey(d => d.UniversityId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_University_AdmissionInformation_University");
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

                entity.Property(e => e.RoleId).HasColumnName("Role_Id");

                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_User_Role");
            });

            modelBuilder.Entity<UserMajor>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.MajorId });

                entity.ToTable("User_Major");

                entity.Property(e => e.UserId).HasColumnName("User_Id");

                entity.Property(e => e.MajorId).HasColumnName("Major_Id");

                entity.HasOne(d => d.Major)
                    .WithMany(p => p.UserMajors)
                    .HasForeignKey(d => d.MajorId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_User_Major_Major");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserMajors)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_User_Major_User");
            });

            modelBuilder.Entity<UserUniversity>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.UniversityId });

                entity.ToTable("User_University");

                entity.Property(e => e.UserId).HasColumnName("User_Id");

                entity.Property(e => e.UniversityId).HasColumnName("University_Id");

                entity.HasOne(d => d.University)
                    .WithMany(p => p.UserUniversities)
                    .HasForeignKey(d => d.UniversityId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_User_University_University");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserUniversities)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_User_University_User");
            });

            modelBuilder.Entity<WeightNumber>(entity =>
            {
                entity.ToTable("WeightNumber");

                entity.Property(e => e.MajorId).HasColumnName("Major_Id");

                entity.Property(e => e.SubjectGroupId).HasColumnName("SubjectGroup_Id");

                entity.Property(e => e.SubjectId).HasColumnName("Subject_Id");

                entity.HasOne(d => d.Major)
                    .WithMany(p => p.WeightNumbers)
                    .HasForeignKey(d => d.MajorId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_WeightNumber_Major");

                entity.HasOne(d => d.SubjectGroup)
                    .WithMany(p => p.WeightNumbers)
                    .HasForeignKey(d => d.SubjectGroupId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_WeightNumber_SubjectGroup");

                entity.HasOne(d => d.Subject)
                    .WithMany(p => p.WeightNumbers)
                    .HasForeignKey(d => d.SubjectId)
                    .HasConstraintName("FK_WeightNumber_Subject");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
