using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


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
