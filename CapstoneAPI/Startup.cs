using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CapstoneAPI.Helpers;
using CapstoneAPI.Models;
using CapstoneAPI.Repositories;
using CapstoneAPI.Services.Major;
using CapstoneAPI.Services.Subject;
using CapstoneAPI.Services.SubjectGroup;
using CapstoneAPI.Services.Test;
using CapstoneAPI.Services.TestSubmission;
using CapstoneAPI.Services.University;
using CapstoneAPI.Services.User;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace CapstoneAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.GetApplicationDefault(),
            });
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<CapstoneDBContext>(options => options.UseSqlServer(Configuration.GetConnectionString("CapstoneDB")));
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
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

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
