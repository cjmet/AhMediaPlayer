using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;
using Microsoft.EntityFrameworkCore.InMemory;
using WinRT;


namespace DataLibrary
{
    public class PlaylistContext : DbContext
    {
        public DbSet<Playlist> Playlists { get; set; }
        //public DbSet<Song> Songs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=c:\\tmp\\ahm_playlists.db");
            //optionsBuilder.UseInMemoryDatabase("playlists");
        }

        public void ResetDb()
        {
            Database.EnsureDeleted();
            Database.EnsureCreated();
        }


        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity<Playlist>().HasKey(p => p.Id);
        //    modelBuilder.Entity<Song>().HasKey(s => s.Id);
        //}


    }
}
