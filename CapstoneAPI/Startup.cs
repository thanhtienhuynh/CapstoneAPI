using CapstoneAPI.CronJobs;
using CapstoneAPI.DataSets;
using CapstoneAPI.DataSets.Email;
using CapstoneAPI.Helpers;
using CapstoneAPI.Models;
using CapstoneAPI.Repositories;
using CapstoneAPI.Services.Article;
using CapstoneAPI.Services.Crawler;
using CapstoneAPI.Services.Email;
using CapstoneAPI.Services.FollowingDetail;
using CapstoneAPI.Services.Major;
using CapstoneAPI.Services.Rank;
using CapstoneAPI.Services.Subject;
using CapstoneAPI.Services.SubjectGroup;
using CapstoneAPI.Services.Test;
using CapstoneAPI.Services.TestSubmission;
using CapstoneAPI.Services.TrainingProgram;
using CapstoneAPI.Services.University;
using CapstoneAPI.Services.User;
using CapstoneAPI.Services.UserMajorDetail;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace CapstoneAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            string path = Path.Combine(Path
                .GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"FirebaseKey\capstone-7071e-firebase-adminsdk-umiw1-2c95fcab0a.json");
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);

            FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.GetApplicationDefault(),
            });
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<CapstoneDBContext>(options => options.UseSqlServer(Configuration.GetConnectionString("CapstoneDB")).EnableSensitiveDataLogging());
            services.AddControllers();
            services.AddControllersWithViews()
                   .AddNewtonsoftJson(options =>
                   options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
               );

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
               .AddJwtBearer(cfg =>
               {
                   cfg.RequireHttpsMetadata = false;
                   cfg.SaveToken = true;

                   cfg.TokenValidationParameters = new TokenValidationParameters()
                   {
                       ValidateIssuerSigningKey = true,
                       IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["AppSettings:JwtSecret"])),
                       ValidateIssuer = true,
                       ValidIssuer = AppSettings.Settings.Issuer,
                       ValidateAudience = true,
                       ValidAudience = AppSettings.Settings.Audience,
                       RequireExpirationTime = false
                   };
               });

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            AddServicesScoped(services);
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddCors();

            services.AddSingleton<IJobFactory, SingletonJobFactory>();
            services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();

            // Add our job
            services.AddSingleton<ArticleCrawlerCronJob>();
            services.AddSingleton(new JobSchedule(
                jobType: typeof(ArticleCrawlerCronJob),
                cronExpression: "0 */30 * ? * *"));
            services.AddHostedService<QuartzHostedService>();
            services.AddSwaggerGen();
            var mailsettings = Configuration.GetSection("MailSettings");  // read config
            services.Configure<EmailSetting>(mailsettings);
            services.AddTransient<IEmailService, EmailService>();
        }

        private void AddServicesScoped(IServiceCollection services)
        {
            services.AddScoped<ISubjectGroupService, SubjectGroupService>();
            services.AddScoped<ISubjectService, SubjectService>();
            services.AddScoped<IUniversityService, UniversityService>();
            services.AddScoped<ITestService, TestService>();
            services.AddScoped<ITestSubmissionService, TestSubmissionService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IMajorService, MajorService>();
            services.AddScoped<IArticleCrawlerService, ArticleCrawlerService>();
            services.AddScoped<ITrainingProgramService, TrainingProgramService>();
            services.AddScoped<IArticleService, ArticleService>();
            services.AddScoped<IFollowingDetailService, FollowingDetailService>();
            services.AddScoped<IRankService, RankService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "MOHS API");
                c.RoutePrefix = string.Empty;
            });
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();
            app.UseAuthentication();

            app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
