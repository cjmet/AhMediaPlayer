using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DataLibrary;

namespace DataLibrary
{
    public interface ISongRepository
    {
        public List<Song> GetSongs();
        public void AddSong(Song song);
    }
}
