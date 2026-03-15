using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RecruitmentInterviewManagementSystem.API.DI;
using RecruitmentInterviewManagementSystem.Applications.Features.Auth;
using RecruitmentInterviewManagementSystem.Applications.Features.BookingInterviewSlot.Interfaces;
using RecruitmentInterviewManagementSystem.Applications.Features.Interface;
using RecruitmentInterviewManagementSystem.Applications.Features.JobPost.Interface;
using RecruitmentInterviewManagementSystem.Applications.Features.JobPost.Services;
using RecruitmentInterviewManagementSystem.Applications.Features.JobPostDetail.Interface;
using RecruitmentInterviewManagementSystem.Applications.Interface;
using RecruitmentInterviewManagementSystem.Domain.InterfacesRepository;
using RecruitmentInterviewManagementSystem.Infastructure.MinIO;
using RecruitmentInterviewManagementSystem.Infastructure.Repository;
using RecruitmentInterviewManagementSystem.Infastructure.ServiceImplement;
using RecruitmentInterviewManagementSystem.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using RecruitmentInterviewManagementSystem.Infastructure.Workers;

using Minio;
using PayOS;
using RecruitmentInterviewManagementSystem.Infastructure.HubPayment;
using RecruitmentInterviewManagementSystem.Applications.TaiOrXiuFeature.Workers;
using RecruitmentInterviewManagementSystem.Applications.TaiOrXiuFeature.HubResult;
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
                    policy.WithOrigins("http://localhost:5173",
                    "https://itlocak.xyz",  // Bắt buộc https và không có / ở cuối
                    "https://103.161.119.162") // Nếu IP cũng có SSL, nếu không thì bỏ dòng này
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
                });
            });
            builder.Services.AddSignalR();
            builder.Services.AddControllers();

            
            builder.Services.Scan(scan => scan
                 .FromAssemblyOf<ApplicationMarker>()
                 .AddClasses(classes => classes.Where(type => !typeof(Microsoft.Extensions.Hosting.IHostedService).IsAssignableFrom(type)))
                 .AsImplementedInterfaces()
                 .WithScopedLifetime());
          

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddMinio(configureClient => configureClient
                 .WithEndpoint("103.161.119.162:9000")
                 .WithCredentials("admin", "2hondaicodon") 
                 .WithSSL(false)
                 .Build());
            builder.Services.AddScoped<IMinIOCV, MinIOfaketopcv>();
            builder.Services.AddHostedService<TakePlaceGame>();
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

                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];
                            var path = context.HttpContext.Request.Path;

                            if (!string.IsNullOrEmpty(accessToken) &&
                               (path.StartsWithSegments("/paymentHub") ||
                                path.StartsWithSegments("/taixiu-hub")))
                            {
                                context.Token = accessToken;
                            }

                            return Task.CompletedTask;
                        }
                    };


                });

            //builder.Services.AddHostedService<AutoUnPost>();

            builder.Services.AddSingleton<PayOSClient>(sp =>
            {
                return new PayOSClient(new PayOSOptions
                {
                    ClientId = builder.Configuration["ClientIDPayOS"],
                    ApiKey = builder.Configuration["ApiKeytPayOS"],
                    ChecksumKey = builder.Configuration["ChecksumKeyPOS"]
                });
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
            app.MapHub<TaiXiuHub>("/taixiu-hub");
            app.MapHub<PaymentHub>("/paymentHub");
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.MapControllers();
            app.MapFallbackToFile("index.html");
            app.Run();
        }
    }
}