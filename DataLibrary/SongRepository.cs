using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using static AngelHornetLibrary.AhLog;


namespace DataLibrary
{

    public class SongRepository : ISongRepository
    {
        private readonly PlaylistContext _context;
        public SongRepository(PlaylistContext context)
        {
            _context = context;
        }

        public async Task<int> AddSongAsync(Song song)
        {
            await _context.Songs.AddAsync(song);
            return _context.SaveChanges();
        }
        public async Task<int> DeleteSongAsync(int songId)
        {
            int result = 0;
            Song song = await GetSongByIdAsync(songId);
            if (song != null)
            {
                result = await _context.Songs.Where(s => s.Id == songId).ExecuteDeleteAsync();
            }
            return result;
        }
        public async Task<int> UpdateSongAsync(int Id, Song song)
        {
            var results = await _context.Songs.Where(s => s.Id == Id).ExecuteUpdateAsync(setters => setters
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
                 .SetProperty(s => s.Star, song.Star)
                 );

            return results;
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
            if (search != null)
            {
                search = search.ToLower();
                return await _context.Songs.Where( s => 
                s.Title.ToLower().Contains(search)      ||
                s.Artist.ToLower().Contains(search)     ||
                s.Album.ToLower().Contains(search)      ||
                s.Band.ToLower().Contains(search)       ||
                s.Genre.ToLower().Contains(search)      || 
                s.PathName.ToLower().Contains(search)   )  
                .OrderBy(s => s.AlphaTitle).ToListAsync();
            }
            return new List<Song>();
        }
        public async Task<List<Song>> SearchQuery(string By, string Search)
        {
            var _by = By.ToLower();
            _by = By.Substring(0, 1).ToUpper() + By.Substring(1);  // Pascal Case
            var _search = Search.ToLower();
            var _db = _context;
            List<Song> _selectionSet = new List<Song>(); 

            if (Search == "ALL") { _search = ""; }
            if (Search == "NULL")
            {
                _search = "";
                if (_by == "Any" || _by == "Title") { _selectionSet = _selectionSet.UnionBy(_db.Songs.Where(s => s.Title.ToLower() == ""), s => s.Id).ToList(); }
                if (_by == "Any" || _by == "Artist") { _selectionSet = _selectionSet.UnionBy(_db.Songs.Where(s => s.Artist == null || s.Artist.ToLower() == ""), s => s.Id).ToList(); }
                if (_by == "Any" || _by == "Album") { _selectionSet = _selectionSet.UnionBy(_db.Songs.Where(s => s.Album == null || s.Album.ToLower() == ""), s => s.Id).ToList(); }
                if (_by == "Any" || _by == "Band") { _selectionSet = _selectionSet.UnionBy(_db.Songs.Where(s => s.Band == null || s.Band.ToLower() == ""), s => s.Id).ToList(); }
                if (_by == "Any" || _by == "Genre") { _selectionSet = _selectionSet.UnionBy(_db.Songs.Where(s => s.Genre == null || s.Genre.ToLower() == ""), s => s.Id).ToList(); }
                if (_by == "Any" || _by == "Path") { _selectionSet = _selectionSet.UnionBy(_db.Songs.Where(s => s.PathName.ToLower() == ""), s => s.Id).ToList(); }
            }
            else
            {
                
                
                if (_by == "Any" || _by == "Title") { _selectionSet = _selectionSet.UnionBy(_db.Songs.Where(s => s.Title.ToLower().Contains(_search)), s => s.Id).ToList(); }
                if (_by == "Any" || _by == "Artist") { _selectionSet = _selectionSet.UnionBy(_db.Songs.Where(s => s.Artist.ToLower().Contains(_search)), s => s.Id).ToList(); }
                if (_by == "Any" || _by == "Album") { _selectionSet = _selectionSet.UnionBy(_db.Songs.Where(s => s.Album.ToLower().Contains(_search)), s => s.Id).ToList(); }
                if (_by == "Any" || _by == "Band") { _selectionSet = _selectionSet.UnionBy(_db.Songs.Where(s => s.Band.ToLower().Contains(_search)), s => s.Id).ToList(); }
                if (_by == "Any" || _by == "Genre") { _selectionSet = _selectionSet.UnionBy(_db.Songs.Where(s => s.Genre.ToLower().Contains(_search)), s => s.Id).ToList(); }
                if (_by == "Any" || _by == "Path") { _selectionSet = _selectionSet.UnionBy(_db.Songs.Where(s => s.PathName.ToLower().Contains(_search)), s => s.Id).ToList(); }
            }
            if (!(new string[] { "Any", "Title", "Artist", "Album", "Band", "Genre", "Path" }.Contains(_by))) LogWarn($"Invalid SearchBy: [{_by}]");

            LogDebug($"SongRepository.SearchQuery: [{By}] Search: [{Search}] Count: [{_selectionSet.Count}]");
            return _selectionSet.OrderBy(s => s.AlphaTitle).ToList();
        }
        
        // Tuples are as much trouble as cats!
        public async Task<(List<Song>, string, string)> AdvancedSearchRepository(List<Song> _currentSet, string _searchString, string _searchBy = "Any", string _searchAction = "SEARCH")
        {
            List<Song> _results = new List<Song>();
            if (_currentSet == null) _currentSet = new List<Song>();
            if (_searchString == null) _searchString = "";
            if (_searchBy == null) _searchBy = "Any";
            if (_searchAction == null) _searchAction = "SEARCH";
            
            (_results, _searchBy, _searchAction) = DataLibraryAdvancedSearch.AdvancedSearch(_currentSet, _searchString, _searchBy, _searchAction, _context);
            
            if (_results == null) _results = new List<Song>();
            if (_searchBy == null) _searchBy = "Any";
            if (_searchAction == null) _searchAction = "SEARCH";

            return (_results, _searchBy, _searchAction);
        }

        public void DbContextChangeTrackerClear()
        {
            _context.ChangeTracker.Clear();
        }
    }

}
