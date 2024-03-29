﻿// Ignore Spelling: Playlists

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using static AngelHornetLibrary.AhLog;
using AhConfig;

namespace DataLibrary
{
    public class PlaylistContext : DbContext
    {
        public DbSet<Playlist> Playlists { get; set; }
        public DbSet<Song> Songs { get; set; }
        public string DbPath { get; private set; }
        public bool VerboseSQL { get; set; } = false;
        private static bool _quiet = false;

        public PlaylistContext()
        {
            // The Win11 and Maui ApplicationData folders are different. Use .Desktop instead.  
            var path = AppData.AppDataPath;

            Directory.CreateDirectory(path);
            File.SetAttributes(path, FileAttributes.Hidden);
            DbPath = Path.Join(path, Const.MusicDbName);
            if (!_quiet || VerboseSQL)
            {
                LogDebug($"***  DbPath: {DbPath}");
                LogDebug($"     Context: {this.ContextId}");
                _quiet = true;
            }
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<Playlist>().HasMany(p => p.Songs).WithMany(s => s.Playlists);
            modelBuilder.Entity<Song>().HasMany(s => s.Playlists).WithMany(p => p.Songs);
        }





        private void ConsoleLog(string logMessage)
        {
            if (VerboseSQL)
            {
                LogDebug(logMessage);
            }
        }

    }
}
