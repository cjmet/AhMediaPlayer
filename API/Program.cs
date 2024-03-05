using AhConfig;
using AngelHornetLibrary;
using API;
using DataLibrary;
using static AngelHornetLibrary.AhLog;


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

            var app = builder.Build();

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
