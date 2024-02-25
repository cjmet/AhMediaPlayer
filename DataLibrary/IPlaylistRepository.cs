using Microsoft.EntityFrameworkCore;

namespace DataLibrary
{
    public interface IPlaylistRepository
    {

        public Task AddPlaylistAsync(Playlist playlist);
        public Task DeletePlaylistAsync(int playlistId);
        public Task UpdatePlaylistAsync(Playlist playlist);
        public Task<Playlist> GetPlaylistByIdAsync(int playlistId);
        public Task<List<Playlist>> GetAllPlaylistsAsync();
        public Task AddSongToPlaylistAsync(int playlistId, Song song);
        public Task RemoveSongFromPlaylist(int playlistId, Song song);

    }
}
