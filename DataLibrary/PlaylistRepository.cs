﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


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