using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RecruitmentInterviewManagementSystem.Applications.Features.Auth;
using RecruitmentInterviewManagementSystem.Applications.Features.BookingInterviewSlot.Interfaces;
using RecruitmentInterviewManagementSystem.Applications.Features.Interface;
using RecruitmentInterviewManagementSystem.Applications.Features.JobPost.Interface;
using RecruitmentInterviewManagementSystem.Applications.Features.JobPost.Services;
using RecruitmentInterviewManagementSystem.Applications.Features.JobPostDetail.Interface;
using RecruitmentInterviewManagementSystem.Applications.Interface;
using RecruitmentInterviewManagementSystem.Domain.InterfacesRepository;
using RecruitmentInterviewManagementSystem.Infastructure.Repository;
using RecruitmentInterviewManagementSystem.Infastructure.ServiceImplement;
using RecruitmentInterviewManagementSystem.Models;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using RecruitmentInterviewManagementSystem.API.DI;
namespace RecruitmentInterviewManagementSystem.Start
{
    public class Program
    {
        public static void Main(string[] args)
        {

            DotNetEnv.Env.Load();

            var builder = WebApplication.CreateBuilder(args);


            builder.Services.AddDbContext<FakeTopcvContext>(options =>
            {

                options.UseSqlServer(builder.Configuration["SQLURL"]);
            });


            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });


            builder.Services.AddControllers();

            builder.Services.Scan(scan => scan
                 .FromAssemblyOf<ApplicationMarker>()
                 .AddClasses()
                 .AsImplementedInterfaces()
                 .WithScopedLifetime());
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();


            var jwtSecret = builder.Configuration["Authentication:Jwt:Secret"]
                            ?? throw new Exception("JWT Secret not configured");
            var jwtIssuer = builder.Configuration["Authentication:Jwt:Issuer"];
            var jwtAudience = builder.Configuration["Authentication:Jwt:Audience"];
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer = jwtIssuer,
                        ValidAudience = jwtAudience,

                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtSecret)
                        ),

                        RoleClaimType = ClaimTypes.Role,
                        NameClaimType = JwtRegisteredClaimNames.Email
                    };
                });
            var app = builder.Build();
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseCors("AllowFrontend");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}