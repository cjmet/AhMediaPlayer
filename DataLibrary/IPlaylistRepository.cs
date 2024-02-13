using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DataLibrary;


namespace DataLibrary
{
    public interface IPlaylistRepository 
    {
        public List<Playlist> GetPlaylists();
        public void AddPlaylist(Playlist playlist);
    }
}
