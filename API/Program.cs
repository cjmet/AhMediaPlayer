// Nuget: Microsoft.AspNetCore.Authentication.Negotiate
using Microsoft.AspNetCore.Authentication.Negotiate;
// Nuget: Microsoft.AspNetCore.Identity.EntityFrameworkCore
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
// Nuget: Microsoft.EntityFrameworkCore.Sqlite
using Microsoft.EntityFrameworkCore.Sqlite;
// Nuget: Microsoft.EntityFrameworkCore.InMemory
using Microsoft.EntityFrameworkCore.InMemory;


using AhConfig;
using AngelHornetLibrary;
using DataLibrary;
using static AngelHornetLibrary.AhLog;
using ApiEndpoints;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CommonNet8;
using static CommonNet8.SearchForMusicFiles;

using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol.Plugins;


namespace API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            AhLog.Start((Serilog.Events.LogEventLevel)Const.MinimumLogLevel, "ApiLog.log");
            if (Const.ApiAllowMusicSearch)
            {
                var _privateDbContext = new PlaylistContext();
                var task = new Task(async () =>
                {
                    LogMsg("Api Init: Searching for Music");
                    await SearchUserProfileMusic(_privateDbContext, new ReportByAction(LogTrace));
                    foreach (var _path in Const.ApiMusicSearchPaths)
                    {
                        await SearchUserProfileMusic(_privateDbContext, new ReportByAction(LogTrace), _path);
                    }
                }, TaskCreationOptions.LongRunning);
                task.Start();
            }

            LogMsg("Starting API");
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddDbContext<PlaylistContext>();
            builder.Services.AddTransient<IPlaylistRepository, PlaylistRepository>();
            builder.Services.AddTransient<ISongRepository, SongRepository>();
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Insert Authentication and Authorization code here
            builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
                .AddIdentityCookies();
            builder.Services.AddAuthorizationBuilder();

            {   // cjm - why did this not work when written the other way like in the Maui App?
                var _path = AppData.AppDataPath;
                Directory.CreateDirectory(_path);
                File.SetAttributes(_path, FileAttributes.Hidden);
                _path = Path.Join(_path, Const.ApiIdDbName);
                LogDebug($"***  DbPath: {_path}");
                builder.Services.AddDbContext<ApiDbContext>(options => options.UseSqlite($"Data Source={_path}"));
                // The following is unnecessarily complicated. 
                var _dbContext = new ApiDbContext(new DbContextOptionsBuilder<ApiDbContext>().UseSqlite($"Data Source={_path}").Options);
                _dbContext.Database.EnsureCreated();
            }

            builder.Services.AddIdentityCore<MyUser>().AddEntityFrameworkStores<ApiDbContext>().AddApiEndpoints();
            // /Authentication and Authorization code

            var app = builder.Build();

            {
                // cjm - I'm assuming this doesn't work here because it's before app.run()?
                // var _dbContext = app.Services.GetService<ApiDbContext>();  
                // _dbContext.Database.EnsureCreated();
            }

            // Authenticate and Authorize
            app.MapIdentityApi<MyUser>();
            // /Authenticate and Authorize

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.MapSongEndpoints();

            app.MapPlaylistEndpoints();

            app.MapPost("/logout", async (SignInManager<MyUser> signInManager, [FromBody] object empty) =>
            {
                if (empty != null)
                {
                    await signInManager.SignOutAsync();
                    return Results.Ok();
                }
                return Results.Unauthorized();
            })
             .WithName("Logout")
             .WithOpenApi()
             .RequireAuthorization();


            app.Run();
        }
    }
}
