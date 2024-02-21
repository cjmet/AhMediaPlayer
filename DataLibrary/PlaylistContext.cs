// Ignore Spelling: Playlists

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using static AngelHornetLibrary.AhLog;


namespace DataLibrary
{
    public class PlaylistContext : DbContext
    {
        private const string dbName = "test_playlists.db";
        public DbSet<Playlist> Playlists { get; set; }
        public DbSet<Song> Songs { get; set; }
        public string DbPath { get; private set; }
        public bool VerboseSQL { get; set; } = false;

        public PlaylistContext()
        {
            // The Win11 and Maui ApplicationData folders are different. Use .Desktop instead.  // cjm - change this later to use the correct folder.
#if DEBUG
            var folder = Environment.SpecialFolder.DesktopDirectory;
#else
            var folder = Environment.SpecialFolder.ApplicationData;
#endif
            var path = Environment.GetFolderPath(folder);
            DbPath = Path.Join(path, dbName);
            LogInfo($"***  DbPath: {DbPath}");
            LogInfo($"     Context: {this.ContextId}");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                //.UseSqlite($"Data Source=c:\\tmp\\test.db") // cjm - hard coded path works, but the DbPath does not when running as an app.
                .UseSqlite($"Data Source={DbPath}") // cjm
                .EnableSensitiveDataLogging()
                .LogTo(ConsoleLog,
                new[] { DbLoggerCategory.Database.Command.Name },
                LogLevel.Information,
                DbContextLoggerOptions.None
                 );
        }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity<Playlist>().HasKey(p => p.Id);
        //    //modelBuilder.Entity<Song>().HasKey(s => s.Id);
        //}





        private void ConsoleLog(string logMessage)
        {
            if (VerboseSQL)
            {
                LogInfo(logMessage);
            }
        }

    }
}
