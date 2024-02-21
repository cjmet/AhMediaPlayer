namespace DataLibrary
{
    public class SongRepository : ISongRepository
    {
        public List<Song> GetSongs()
        {
            return new List<Song>();
        }
        public void AddSong(Song song)
        {
            throw new NotImplementedException();
        }
    }
}
