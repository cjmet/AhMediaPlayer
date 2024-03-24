using DataLibrary;
using Microsoft.EntityFrameworkCore;
using static AngelHornetLibrary.AhLog;

namespace AhMediaPlayer
{
    public partial class MainPage : ContentPage
    {
        private void TestPlaylist_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var _playList = (Playlist)e.SelectedItem;
            var _index = e.SelectedItemIndex;
            var _listView = (ListView)sender;
            _listView.Focus();

            SetEditBarState();
            if (_playList != null)
            {
                LogMsg($"Playlist Selected: {_playList.Id}   {_playList.Name}   {_playList.Description}");
                ChangePlaylist(_playList.Id);
            }
            else LogMsg("Playlist is null");
        }
        private async void ChangePlaylist(int Id)
        {
            Playlist? playlist = await _dbContext.Playlists.Where(p => p.Id == Id).Include(p => p.Songs).FirstOrDefaultAsync();
            if (playlist != null) LogMsg($"ChangePlaylist: {playlist.Id}   {playlist.Name}   {playlist.Description}");
            else LogWarning("WARN[124] playlist is null");

            await DispatchSonglist(playlist?.Songs); // cjm2
        }
        private Boolean Gui_SaveAs = true;        // SaveAS vs Save vs Edit
        private void SaveAsPlaylistGui_Clicked(object sender, EventArgs e)
        {
            Gui_SaveAs = true;
            var _sender = (Button)sender;

            if (SaveAsPlaylistFrame.IsVisible) this.Dispatcher.Dispatch(() =>
            {
                SaveAsPlaylistFrame.IsVisible = false;
                SaveAsPlaylistName.Text = "";
                SaveAsPlaylistDesc.Text = "";
            });

            else this.Dispatcher.Dispatch(() =>
            {
                SaveAsPlaylistFrame.IsVisible = true;
                SaveAsPlaylistName.Text = "";
                SaveAsPlaylistDesc.Text = "";
            });
            return;

        }
        private async void DoSavePlaylistFrame_Clicked(object sender, EventArgs e)      // cj - Currently Disabled ... probably not coming back?
        {
            Enable_Gui(false);
            await Task.Delay(1);

            var _sender = (Button)sender;

            var _name = SaveAsPlaylistName.Text;
            var _desc = SaveAsPlaylistDesc.Text;

            if (_name == null || _name == "")
            {
                var _lists = await _dbContext.Playlists.ToListAsync();
                var _last = _lists?.LastOrDefault();
                if (_last != null && _last.Id != null && _last.Id > 0)
                {
                    if (Gui_SaveAs) _name = $"Playlist {_last.Id + 1}";
                    else
                    {
                        var _playlist = (Playlist)TestPlaylist.SelectedItem;
                        if (_playlist == null) { Enable_Gui(true); return; }
                        _name = $"Playlist {_playlist.Id}";
                    }
                }
                else
                {
                    LogError("Playlists are null.");
                    _name = null;
                }
            }
            if (_desc == null) _desc = "";



            if (_name != null)
            {
                Playlist _playlist = new Playlist();
                if (Gui_SaveAs)
                {

                    _playlist = new Playlist { Name = _name, Description = _desc, Songs = new List<Song>() };
                    _dbContext.Playlists.Add(_playlist);
                    List<Song>? _songList = new List<Song>();
                    await Task.Run(() => _songList = TestSonglist.ItemsSource.Cast<Song>().ToList());
                    if (_songList == null) _songList = new List<Song>();
                    int i = 0;
                    foreach (Song _song in _songList)
                        if (_song != null)
                        {
                            var _addSong = _dbContext.Songs.Find(_song.Id);             // EF Core Tracking I Hate You.
                            if (_addSong != null) _playlist.Songs.Add(_addSong);
                            if (i++ % 400 == 0) await Task.Delay(1);                    // roughly 25 fps on my machine.
                            if (i % 10000 == 0) LogMsg($"Adding Songs: {i}");
                        }
                    _dbContext.SaveChanges();
                }
                else
                {
                    _playlist = (Playlist)TestPlaylist.SelectedItem;
                    if (_playlist == null) { Enable_Gui(true); return; }
                    _playlist.Name = _name;
                    _playlist.Description = _desc;
                    _dbContext.SaveChanges();
                }

                LogMsg($"Playlist Saved: [{_playlist.Id}] {_playlist.Name} - {_playlist.Description}");
                var _playlists = _dbContext.Playlists.ToList();
                _playlists = _playlists.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase).ToList();
                this.Dispatcher.Dispatch(async () =>
                {
                    TestPlaylist.ItemsSource = _playlists;
                    await Task.Delay(1);
                    TestPlaylist.SelectedItem = _playlist;
                    await Task.Delay(1);
                    TestPlaylist.ScrollTo(_playlist, ScrollToPosition.Start, true);
                });
            }

            this.Dispatcher.Dispatch(() =>
            {
                SaveAsPlaylistFrame.IsVisible = false;
                SaveAsPlaylistName.Text = "";
                SaveAsPlaylistDesc.Text = "";
            });

            Enable_Gui(true);
        }
        private async void DeletePlaylistImproved(object sender, EventArgs e)
        {
            // Removing the songs first, with a smart selective list, makes the delete playlist 100x faster.
            // So we're reusing what we learned in Holy Swiss Cheese, then doing the playlist delete.
            LogMsg("Intercepting DeletePlaylistGui_Clicked");
            var _songs = (List<vSong>)TestSonglist.ItemsSource;
            if (_songs == null) return;

            Enable_Gui(false);
            await AddRemoveSongList(sender, e, _songs, false);

            var _appPlaylist = (Playlist)TestPlaylist.SelectedItem;
            var _playlistID = _appPlaylist.Id;

            var _playlists = await _dbContext.Playlists.ToListAsync();
            _playlists = _playlists.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase).ToList();
            var _playListIndex = _playlists.IndexOf(_appPlaylist);

            var _dbPlaylist = await _dbContext.Playlists.Where(p => p.Id == _playlistID).FirstOrDefaultAsync();
            if (_dbPlaylist == null || _playlistID <= 1) { Enable_Gui(true); return; }

            await Task.Run(async () =>
            {
                LogDebug($"Removing[967] ... ");
                _dbContext.Playlists.Remove(_dbPlaylist);
                LogDebug($"Removed[968] ... ");
                await Task.Delay(1);
                LogDebug($"Saving[973] ... ");
                _dbContext.SaveChanges();
                LogDebug($"Saved[975] ... ");
            });

            TestPlaylist.SelectedItem = null;
            _playlists = await _dbContext.Playlists.ToListAsync();
            _playlists = _playlists.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase).ToList();
            var _index = _playListIndex - 1;
            _index = _index < 0 ? 0 : _index;
            _appPlaylist = _playlists.ElementAtOrDefault(_index);
            LogDebug($"Dispatching[976] ... ");
            await this.Dispatcher.DispatchAsync(async () =>
            {
                TestPlaylist.ItemsSource = _playlists;
                await Task.Delay(1);
                if (_appPlaylist != null)
                {
                    TestPlaylist.ScrollTo(_appPlaylist, ScrollToPosition.Start, true);
                    TestPlaylist.SelectedItem = _appPlaylist;
                }
            });

            Enable_Gui(true);

            return;
        }
        private async void DeletePlaylistGui_Clicked(object sender, EventArgs e)
        {

            var _sender = (Button)sender;
            var _playlist = (Playlist)TestPlaylist.SelectedItem;
            if (_playlist == null || _playlist.Id == 1) return;

            bool answer = await DisplayAlert("Delete Playlist", _playlist.Name, "Delete", "Cancel");

            // cj
            // Intercept this and lets see if we can make it better.
            if (answer) DeletePlaylistImproved(sender, e);
            return;
        }
        private async Task AddRemoveSongList(object sender, EventArgs e, List<vSong>? _songlist, Boolean _addSongs)
        {
            // cj - Holy Swiss Cheese!  
            // Adding a song that's already added causes a MASSIVE slowdown and even causes the SaveChangesAsync to hard lock sometimes.
            // It might be better to use SaveChanges() no Async.
            // However, making a 'smart' list of changes only, avoids the 'setting already set' problem which in turn avoids the massive slowdown and hard lock.
            // Just 30 or 40 songs 'setting already set' was enough to cause a hard lock.
            // ... now to test more extensively.

            LogMsg("Intercepting Add/Remove Song");
            var _sender = (Button)sender;
            var _playlist = (Playlist)TestPlaylist.SelectedItem;
            var _playlistId = _playlist.Id;
            // Pass this in instead, then I can use this method for other things as well. //Removed: var _songlist = (List<vSong>)TestSonglist.ItemsSource; 
            var _songlistIds = _songlist.Select(s => s.Id).ToList();
            if (_playlist == null || _playlist.Id <= 1) return;
            if (_songlist == null) return;

            await Task.Delay(1);
            if (_addSongs)
            {
                var _existingIds = await _dbContext.Songs.Where(s => s.Playlists.Any(p => p.Id == _playlistId)).Select(s => s.Id).ToListAsync();
                var _newSongIds = _songlistIds.Except(_existingIds).ToList();
                LogMsg($"Adding {_newSongIds.Count} Songs to Playlist (DB)[{_playlistId}]");
                // cj - check for null playlists.  Use "?" notation!
                await _dbContext.Songs.Where(s => _newSongIds.Contains(s.Id)).Include(s => s.Playlists).ForEachAsync(s => { s.Playlists?.Add(_playlist); });
                await Task.Delay(1);

                // Stars are just a visual aid, we don't have to actually set them in the database, just the View.
                LogMsg($"Setting Stars {_newSongIds.Count} Songs to Playlist (local)[{_playlistId}]");
                foreach (var song in _songlist.Where(s => _newSongIds.Contains(s.Id))) song.Star = true;

                LogMsg($"Saving ... ");
                var results = await _dbContext.SaveChangesAsync();
                if (results < _newSongIds.Count) LogError($"ERROR: Saved [{results}] of [{_newSongIds.Count}] ");
                LogMsg($"Saved [{results}] ... ");
            }
            else
            {
                var _existingIds = await _dbContext.Songs.Where(s => s.Playlists.Any(p => p.Id == _playlistId)).Select(s => s.Id).ToListAsync();
                var _removeSongIds = _songlistIds.Intersect(_existingIds).ToList();
                LogMsg($"Removing {_removeSongIds.Count} Songs from Playlist (DB)[{_playlistId}]");
                // cj - check for null playlists.  Use "?" notation!
                await _dbContext.Songs.Where(s => _removeSongIds.Contains(s.Id)).Include(s => s.Playlists).ForEachAsync(s => { s.Playlists?.Remove(_playlist); });
                await Task.Delay(1);

                // Stars are just a visual aid, we don't have to actually set them in the database, just the View.
                LogMsg($"Setting Stars {_removeSongIds.Count} Songs from Playlist (local)[{_playlistId}]");
                foreach (var song in _songlist.Where(s => _removeSongIds.Contains(s.Id))) song.Star = false;

                LogMsg($"Saving ... ");
                var results = await _dbContext.SaveChangesAsync();
                if (results < _removeSongIds.Count) LogError($"ERROR: Saved [{results}] of [{_removeSongIds.Count}] ");
                LogMsg($"Saved [{results}] ... ");
            }
        }
        private async void AddSongsGui_Clicked(object sender, EventArgs e)    // cj 
        {
            // Intercept and lets see if we can do this more efficiently.
            var _songs = (List<vSong>)TestSonglist.ItemsSource;
            if (_songs == null) return;
            Enable_Gui(false);
            await AddRemoveSongList(sender, e, _songs, true);
            Enable_Gui(true);
            return;
        }
        private async void RemoveSongsGui_Clicked(object sender, EventArgs e)
        {
            // Intercept and lets see if we can do this more efficiently.
            var _songs = (List<vSong>)TestSonglist.ItemsSource;
            if (_songs == null) return;
            Enable_Gui(false);
            await AddRemoveSongList(sender, e, _songs, false);
            Enable_Gui(true);
            return;
        }
        private async Task AddRemoveSong(vSong _song, bool _add)
        {
            throw new Exception("This is not used anymore.");
        }
        private void OnStar_Clicked(object sender, EventArgs e)
        {
            var _sender = (Button)sender;
            var _song = (vSong)_sender.BindingContext;
            if (_song == null) return;
            var _playlist = (Playlist)TestPlaylist.SelectedItem;
            if (_playlist == null) return;
            if (_playlist.Id == null) return;
            if (_playlist.Id <= 1) return;

            LogDebug($"Star Song[{_song.Star}]: {_song.Title}");
            _song.Star = !_song.Star;

            var _dbSong = _dbContext.Songs.Include(s => s.Playlists).Where(s => s.Id == _song.Id).FirstOrDefault();
            var _dbPlaylist = _dbContext.Playlists.Where(p => p.Id == _playlist.Id).FirstOrDefault();
            if (_song.Star)
            {
                _dbSong.Playlists.Add(_playlist);
                if (_playlist.Songs != null) _playlist.Songs.Add(_dbSong);
            }
            else
            {
                _dbSong.Playlists.Remove(_playlist);
                if (_playlist.Songs != null) _playlist.Songs.Remove(_dbSong);
            }

            var results = _dbContext.SaveChanges();
            LogMsg($"Id[{_playlist.Id}]: {_playlist.Name}   [{(_song.Star ? "Adding" : "Removing")}]: {_song.Title}   Results: {results}");
        }
        private void EditPlaylistGui_Clicked(object sender, EventArgs e)
        {
            Gui_SaveAs = false;
            var _sender = (Button)sender;
            var _playlist = (Playlist)TestPlaylist.SelectedItem;
            if (_playlist == null || _playlist.Id == 1) return;
            var _name = _playlist.Name;
            var _desc = _playlist.Description;
            if (_name == null) _name = "";
            if (_desc == null) _desc = "";

            if (SaveAsPlaylistFrame.IsVisible) this.Dispatcher.Dispatch(() =>
            {
                SaveAsPlaylistFrame.IsVisible = false;
                SaveAsPlaylistName.Text = "";
                SaveAsPlaylistDesc.Text = "";
            });

            else this.Dispatcher.Dispatch(() =>
            {
                SaveAsPlaylistFrame.IsVisible = true;
                SaveAsPlaylistName.Text = _name;
                SaveAsPlaylistDesc.Text = _desc;
            });
            return;

        }

    }
}
