using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace DataLibrary
{

    public class SongRepository : ISongRepository
    {
        private readonly PlaylistContext _context;
        public SongRepository(PlaylistContext context)
        {
            _context = context;
        }

        public async Task AddSongAsync(Song song)
        {
            await _context.Songs.AddAsync(song);
            _context.SaveChanges();
        }
        public async Task DeleteSongAsync(int songId)
        {
            Song song = await GetSongByIdAsync(songId);
            if (song != null)
            {
                await _context.Songs.Where(s => s.Id == songId).ExecuteDeleteAsync();
            }
        }
        public async Task UpdateSongAsync(Song song)
        {
            await _context.Songs.Where(s => s.Id == song.Id).ExecuteUpdateAsync(setters => setters
                 .SetProperty(s => s.PathName, song.PathName)
                 .SetProperty(s => s.Title, song.Title)
                 .SetProperty(s => s.AlphaTitle, song.AlphaTitle)
                 .SetProperty(s => s.Artist, song.Artist)
                 .SetProperty(s => s.Album, song.Album)
                 .SetProperty(s => s.Band, song.Band)
                 .SetProperty(s => s.Genre, song.Genre)
                 .SetProperty(s => s.Year, song.Year)
                 .SetProperty(s => s.Track, song.Track)
                 .SetProperty(s => s.Length, song.Length)
                 );
        }
        public async Task<Song?> GetSongByIdAsync(int songId)
        {
            return await _context.Songs.FindAsync(songId);
        }
        public async Task<List<Song>> GetAllSongsAsync()
        {
            return await _context.Songs.OrderBy(s => s.AlphaTitle).ToListAsync();
        }
        public async Task<List<Song>> SearchAllSongs(string search)
        {
            var query = _context.Songs.AsQueryable();

            if (search != null)
            {
                search = search.ToLower();
                return await _context.Songs.Where( s => 
                s.Title.ToLower().Contains(search) ||
                s.Artist.ToLower().Contains(search) ||
                s.Album.ToLower().Contains(search) ||
                s.Band.ToLower().Contains(search) ||
                s.Genre.ToLower().Contains(search))
                .OrderBy(s => s.AlphaTitle).ToListAsync();
            }
            return new List<Song>();
        }
    }




}
