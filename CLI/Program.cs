
using DataLibrary;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;



namespace MauiCli
{
    internal class Program
    {
        static IServiceProvider CreateServiceCollection()
        {
            //  When using Dependency Injection,  
            return new ServiceCollection()
                .AddDbContext<PlaylistContext>()
                .BuildServiceProvider();
        }


        static void Main(string[] args) // Main 
        {

            Console.WriteLine("Hello, World!");

            {
                var _dbContext = new PlaylistContext();
                _dbContext.Database.EnsureDeleted();
                _dbContext.Database.EnsureCreated();
                Playlist playlist = new Playlist();
                playlist.Name = "Test Playlist X";
                playlist.Description = "Test Description X";
                _dbContext.Playlists.Add(playlist);

                _dbContext.Playlists.Add(new Playlist
                {
                    Name = "Test Playlist 11",
                    Description = "Test Description 11"
                });
                _dbContext.Playlists.Add(new Playlist
                {
                    Name = "Test Playlist 12",
                    Description = "Test Description 12"
                });
                _dbContext.SaveChanges();
                _dbContext.Dispose();

                // --- 

                Debug.WriteLine("Reading Playlists from Db ...");
                _dbContext = new PlaylistContext();
                var playlists = _dbContext.Playlists.ToList();
                _dbContext.Dispose();
                int i = 0;
                foreach (var p in playlists)
                {
                    Debug.WriteLine($"[{++i}] Playlist: {p.Name} - {p.Description}");
                }
                Debug.WriteLine("...");

            }

        }
    }
}