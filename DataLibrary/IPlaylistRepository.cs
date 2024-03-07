using Microsoft.EntityFrameworkCore;

namespace DataLibrary
{
    public interface IPlaylistRepository
    {

        public Task<int> AddPlaylistAsync(Playlist playlist);
        public Task<int> DeletePlaylistAsync(int playlistId);
        public Task<int> UpdatePlaylistAsync(int Id, Playlist playlist);
        public Task<Playlist> GetPlaylistByIdAsync(int playlistId);
        public Task<Playlist> GetPlaylistWithSongsAsync(int playlistId);
        public Task<List<Playlist>> GetAllPlaylistsAsync();
        public Task<int> AddSongToPlaylistAsync(int playlistId, int songId);
        public Task<int> RemoveSongFromPlaylist(int playlistId, int songId);

    }
}
