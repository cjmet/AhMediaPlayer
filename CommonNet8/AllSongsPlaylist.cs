using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataLibrary;
using Microsoft.EntityFrameworkCore;
using static AngelHornetLibrary.AhLog;

namespace CommonNet8
{
    public class AllSongsPlaylist
    {
        public static async Task UpdateAllSongsPlaylist()
        {
            LogMsg("Updating All Songs Playlist");
            var dbContext = new PlaylistContext();

            var _playlist = await dbContext.Playlists.Where(p => p.Name == " All Songs").Include(p => p.Songs).FirstOrDefaultAsync();
            if (_playlist == null)
            {
                LogDebug("Creating All Songs Playlist");
                _playlist = new Playlist
                {
                    Name = " All Songs",
                    Description = " All Songs",
                    Songs = new List<Song>()
                };
                dbContext.Playlists.Add(_playlist);
            }
            

            LogDebug("Adding Songs to All Songs Playlist");
            await foreach (var song in dbContext.Songs)
            {
                if (song == null || song.PathName == null)
                {
                    LogError("ERROR[038] Song or Pathname is NULL ***");
                    continue;
                }
                if (_playlist.Songs.Where(s => s.PathName == song.PathName).FirstOrDefault() == null)
                {
                    _playlist.Songs.Add(song);
                }
            }
            LogDebug("Saving All Songs Playlist");
            dbContext.SaveChanges();
        }
    }
}
