using Microsoft.EntityFrameworkCore;
using DataLibrary;

namespace DataLibrary
{

    public class PlaylistRepository : IPlaylistRepository
    {
        private readonly PlaylistContext _context;
        public PlaylistRepository(PlaylistContext context)
        {
            _context = context;
        }



        public async Task AddPlaylistAsync(Playlist playlist)
        {
            await _context.Playlists.AddAsync(playlist);
            _context.SaveChanges();
        }
        public async Task DeletePlaylistAsync(int playlistId)
        {
            Playlist playlist = await GetPlaylistByIdAsync(playlistId);

            if (playlist != null)
            {
                await _context.Playlists.Where(p => p.Id == playlistId).ExecuteDeleteAsync();
            }
        }
        public async Task UpdatePlaylistAsync(Playlist playlist)
        {
            await _context.Playlists.Where(p => p.Id == playlist.Id).ExecuteUpdateAsync(setters => setters
                 .SetProperty(p => p.Name, playlist.Name)
                 .SetProperty(p => p.Description, playlist.Description)
                 .SetProperty(p => p.Songs, playlist.Songs)
                 );
        }
        public async Task<Playlist> GetPlaylistByIdAsync(int playlistId)
        {
            return await _context.Playlists.FindAsync(playlistId);
        }
        public async Task<Playlist> GetPlaylistWithSongsAsync(int playlistId)
        {
            return await _context.Playlists.Where(p => p.Id == playlistId).Include(p => p.Songs).FirstOrDefaultAsync();
        }
        public async Task<List<Playlist>> GetAllPlaylistsAsync()
        {
            return await _context.Playlists.Include(p => p.Songs).ToListAsync();
        }
        public async Task AddSongToPlaylistAsync(int playlistId, Song song)
        {
            Playlist playlist = await _context.Playlists.Where(p => p.Id == playlistId).Include(p => p.Songs).FirstOrDefaultAsync();

            if (playlist != null && song != null)
            {
                playlist.Songs.Add(song);
                _context.SaveChanges();
            }
        }
        public async Task RemoveSongFromPlaylist(int playlistId, Song song)
        {
            Playlist playlist = await _context.Playlists.Where(p => p.Id == playlistId).Include(p => p.Songs).FirstOrDefaultAsync();

            if (playlist != null && song != null)
            {
                playlist.Songs.Remove(song);
                _context.SaveChanges();
            }
        }
    }



}
