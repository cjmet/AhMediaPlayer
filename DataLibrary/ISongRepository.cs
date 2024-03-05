namespace DataLibrary
{
    public interface ISongRepository
    {
        public Task<int> AddSongAsync(Song song);
        public Task<int> DeleteSongAsync(int songId);
        public Task<int> UpdateSongAsync(int Id, Song song);
        public Task<Song> GetSongByIdAsync(int songId);
        public Task<List<Song>> GetAllSongsAsync();
        public Task<List<Song>> SearchAllSongs(string search);  // Searching "" will return all songs
        public Task<List<Song>> SearchQuery(string property, string search);
        public Task<(List<Song>, string, string)> AdvancedSearchRepository(List<Song> _currentSet, string _searchString, string _searchBy = "Any", string _searchAction = "SEARCH");

    }
}
