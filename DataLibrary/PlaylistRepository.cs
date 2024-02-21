namespace DataLibrary
{
    public class PlaylistRepository : IPlaylistRepository
    {
        public List<Playlist> GetPlaylists()
        {
            return new List<Playlist>();
        }
        public void AddPlaylist(Playlist playlist)
        {
            throw new NotImplementedException();
        }

    }
}
