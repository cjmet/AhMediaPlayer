﻿using Microsoft.EntityFrameworkCore;
using DataLibrary;

namespace DataLibrary
{

    public class PlaylistRepository : IPlaylistRepository
    {
        private readonly PlaylistContext _context;
        public PlaylistRepository(PlaylistContext context)
        {
            _context = context;
        }



        public async Task<int> AddPlaylistAsync(Playlist playlist)
        {
           await _context.Playlists.AddAsync(playlist);
            return  _context.SaveChanges();
        }
        public async Task<int> DeletePlaylistAsync(int playlistId)
        {
            Playlist playlist = await GetPlaylistByIdAsync(playlistId);
            var _result = 0;
            if (playlist != null)
            {
                _result = await _context.Playlists.Where(p => p.Id == playlistId).ExecuteDeleteAsync();
            }
            return _result;
        }
        public async Task<int> UpdatePlaylistAsync(int Id, Playlist playlist)
        {
            if (playlist.Songs == null) playlist.Songs = new List<Song>();
            // Can NOT use .Include(p => p.songs) in an ExecuteUpdateAsync() call?!?  Probably because it's an attached property, not a direct column?  - cjm
            var _results =  await _context.Playlists.Where(p => p.Id == Id).ExecuteUpdateAsync(setters => setters
                 .SetProperty(p => p.Name, playlist.Name)
                 .SetProperty(p => p.Description, playlist.Description)
                 );

            // Lets trying setting it separately ... That didn't work either.  
            //_results += await _context.Playlists.Include(p => p.Songs).Where(p => p.Id == Id).ExecuteUpdateAsync(setters => setters
            //    .SetProperty(p => p.Songs, playlist.Songs)

            // Lets try setting it directly, synchronously ... That worked. 
            var _playlist = await _context.Playlists.Where(p => p.Id == Id).Include(p => p.Songs).FirstOrDefaultAsync();
            _playlist.Songs = playlist.Songs;
            _results += await _context.SaveChangesAsync();
            return _results;
        }
        public async Task<Playlist> GetPlaylistByIdAsync(int playlistId)
        {
            return await _context.Playlists.FindAsync(playlistId);
        }
        public async Task<Playlist> GetPlaylistWithSongsAsync(int playlistId)
        {
            return await _context.Playlists.Where(p => p.Id == playlistId).Include(p => p.Songs).FirstOrDefaultAsync();
        }
        public async Task<List<Playlist>> GetAllPlaylistsAsync()
        {
            return await _context.Playlists.ToListAsync();
        }
        public async Task AddSongToPlaylistAsync(int playlistId, Song song)
        {
            Playlist playlist = await _context.Playlists.Where(p => p.Id == playlistId).Include(p => p.Songs).FirstOrDefaultAsync();

            if (playlist != null && song != null)
            {
                playlist.Songs.Add(song);
                _context.SaveChanges();
            }
        }
        public async Task RemoveSongFromPlaylist(int playlistId, Song song)
        {
            Playlist playlist = await _context.Playlists.Where(p => p.Id == playlistId).Include(p => p.Songs).FirstOrDefaultAsync();

            if (playlist != null && song != null)
            {
                playlist.Songs.Remove(song);
                _context.SaveChanges();
            }
        }
    }



}
