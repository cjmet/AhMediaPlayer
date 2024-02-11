// Ignore Spelling: Playlists

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Diagnostics;


namespace DataLibrary
{
    public class PlaylistContext : DbContext
    {
        private const string dbName = "test_playlists.db";
        public DbSet<Playlist> Playlists { get; set; }
        public string DbPath { get; private set; }
        public bool VerboseSQL { get; set; } = true;

        public PlaylistContext()
        {
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);
            DbPath = Path.Join(path, dbName);
            ConsoleLog($"\nDbContextId: {this.ContextId}\n");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
             .UseSqlite($"Data Source={DbPath}")
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
                Debug.WriteLine(logMessage);
            }
        }

    }
}
