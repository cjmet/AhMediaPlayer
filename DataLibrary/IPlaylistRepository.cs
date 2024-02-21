namespace DataLibrary
{
    public interface IPlaylistRepository
    {
        public List<Playlist> GetPlaylists();
        public void AddPlaylist(Playlist playlist);
    }
}
