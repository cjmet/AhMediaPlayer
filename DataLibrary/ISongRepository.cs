namespace DataLibrary
{
    public interface ISongRepository
    {
        public Task AddSongAsync(Song song);
        public Task DeleteSongAsync(int songId);
        public Task UpdateSongAsync(Song song);
        public Task<Song> GetSongByIdAsync(int songId);
        public Task<List<Song>> GetAllSongsAsync();
        public Task<List<Song>> SearchAllSongs(string search);  // Searching "" will return all songs

    }
}
