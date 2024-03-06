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
using API;
using DataLibrary;
using static AngelHornetLibrary.AhLog;
using ApiEndpoints;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


namespace API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            AhLog.Start((Serilog.Events.LogEventLevel)Const.MinimumLogLevel);
            LogMsg("API Started");

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
            builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
              .AddNegotiate();

            builder.Services.AddAuthorization(options =>
            {
                // By default, all incoming requests will be authorized according to the default policy.
                options.FallbackPolicy = options.DefaultPolicy;
            });

            builder.Services.AddDbContext<ApiDbContext>( options => options.UseInMemoryDatabase("ApiDb"));

            builder.Services.AddIdentityCore<MyUser>()
                .AddEntityFrameworkStores<ApiDbContext>()
                .AddApiEndpoints();
            // /Authentication and Authorization code

            var app = builder.Build();

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

            app.Run();
        }
    }
}
