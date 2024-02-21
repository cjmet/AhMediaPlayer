namespace DataLibrary
{
    public interface ISongRepository
    {
        public List<Song> GetSongs();
        public void AddSong(Song song);
    }
}
