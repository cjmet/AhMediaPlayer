using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AhConfig;
using AngelHornetLibrary;
using CommonNet8;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Maui.Views;
using DataLibrary;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using static AngelHornetLibrary.AhLog;
using static CommonNet8.AllSongsPlaylist;
using static CommonNet8.SearchForMusicFiles;
using static MauiMediaPlayer.ProgramLogic.StaticProgramLogic;
using static DataLibrary.DataLibraryAdvancedSearch;
using Microsoft.EntityFrameworkCore;




// *** WARNING *** Do NOT trust references numbers when it relates to XAML event handlers.
// await Task.Delay(1);  // This allows the GUI to render a frame, and has been inserted as needed where performance lagged, even in async contexts if it was needed.

namespace MauiMediaPlayer
{
    public partial class MainPage : ContentPage
    {




        private void RandomPersistentLogo(string data, int found)
        {
            LogTrace($"RandomPersistentLogo: {data}"); // cj 
            data = data.ToLower().Trim();
            var num = data.GetHashCode();
            var images = new string[]
            {   // cj - I can't seem to find a way to read these (from the package), so I'm just going to hard code them.
                "angel_hornet_logo_cropped.png", "azrael_lc.png",
                "bad_grim_lc.png", "baxters_lc.png", "big_moon_lc.png",
                "brie_lc.png", "bubby_lc.png", "chex_lc.png", "damnit_gizmo_lc.png",
                "django_lc.png", "dot_lc.png", "fox_lc.png", "herc_lc.png",
                "howard_lc.png", "huggy_lc.png", "kibbs_lc.png", "kissy_lc.png",
                "lucy_lc.png", "luna_lc.png", "nino_lc.png", "pie_lc.png",
                "possum_lc.png", "rey_lc.png", "spicey_lc.png", "stella_lc.png",
                "tiger_lc.png"
            };

            var index = (uint)num % images.Length;
            if (data == "") index = 0;
            LogTrace($"RandomPersistentLogo: {index} / {images.Length}");

            var image = images[index];
            var Name = Path.GetFileNameWithoutExtension(image);
            if (Name == "angel_hornet_logo_cropped") Name = "Angel Hornet";
            var Names = Name.Split('_');
            Name = "";
            foreach (var n in Names)
            {
                if (n.Length >= 1) Name += n.Substring(0, 1).ToUpper() + n.Substring(1) + " ";
            }
            Name = Name.Trim();
            Name = Name.Replace(" Lc", "");                 // Curse you GitHub for making me rename the files so you'd respect lower case!
            LogMsg($"{Name} found you {found} Songs!");

            this.Dispatcher.Dispatch(() =>
            {
                AngelHornetLogo.Source = ImageSource.FromFile(images[index]);
            });
        }

        private async void SearchDirectories_Clicked(object sender, EventArgs e)
        {
            var folder = Environment.SpecialFolder.UserProfile;
            var _tmpPath = Environment.GetFolderPath(folder);
            _tmpPath = Path.Join(_tmpPath, "Music");
            var _searchDir = await FolderPicker.PickAsync(_tmpPath);  // cj 
            if (_searchDir == null || _searchDir.Folder == null) return;
            var _path = _searchDir.Folder.Path;
            LogMsg($"SearchDirectories: {_path}");
            if (_path != "")
            {
                var _progress = new ReportProgressToQueue(_messageQueue);
                _ = SearchUserProfileMusic(_dbContext, _progress, _path);
            }
        }

        private void MenuBox_Clicked(object sender, EventArgs e)
        {
            if (AdvandedSearchFrame.IsVisible) this.Dispatcher.Dispatch(() =>
            {
                AdvandedSearchFrame.IsVisible = false;
                MenuBox.BackgroundColor = Color.Parse("Transparent");
                StandardSearchBar.IsEnabled = true;
            });
            else this.Dispatcher.Dispatch(() =>
            {
                SetEditBarState();
                AdvandedSearchFrame.IsVisible = true;
                MenuBox.BackgroundColor = Color.Parse("LightBlue");
                Searchby.SelectedIndex = 0;
                SearchAction.SelectedIndex = 0;
                StandardSearchBar.IsEnabled = false;
            });
        }
        private void SetEditBarState()
        {
            Playlist? _playlist = (Playlist)TestPlaylist.SelectedItem;
            if (_playlist == null || _playlist.Id == 1) this.Dispatcher.Dispatch(() =>
            {
                AddSongsGui.Opacity = 0.25;
                AddSongsGui.IsEnabled = false;
                RemoveSongsGui.Opacity = 0.25;
                RemoveSongsGui.IsEnabled = false;
                DeletePlaylistGui.Opacity = 0.25;
                DeletePlaylistGui.IsEnabled = false;
                EditPlaylistGui.Opacity = 0.25;
                EditPlaylistGui.IsEnabled = false;
            });
            else if (_playlist.Id > 1) this.Dispatcher.Dispatch(() =>
            {
                AddSongsGui.Opacity = 1;
                AddSongsGui.IsEnabled = true;
                RemoveSongsGui.Opacity = 1;
                RemoveSongsGui.IsEnabled = true;
                DeletePlaylistGui.Opacity = 1;
                DeletePlaylistGui.IsEnabled = true;
                EditPlaylistGui.Opacity = 1;
                EditPlaylistGui.IsEnabled = true;
            });
        }
        private async void SearchBar_SearchButtonPressed(object sender, EventArgs e) => AdvancedSearchBar_SearchButtonPressed(sender, e);
        private async void AdvancedSearchBar_SearchButtonPressed(object sender, EventArgs e)
        {
            var _searchBar = (SearchBar)sender;
            var _searchText = _searchBar.Text;
            if (_searchText == null) _searchText = "";


            string? _by;
            if (Searchby.SelectedItem == null) _by = "Any";
            else _by = Searchby.SelectedItem.ToString().ToLower();

            string? _action;
            if (SearchAction.SelectedItem == null) _action = "Search";
            else _action = SearchAction.SelectedItem.ToString().ToLower();

            List<Song> _currentSet = new List<Song>();
            if (TestSonglist != null && TestSonglist.ItemsSource != null) _currentSet = TestSonglist.ItemsSource.Cast<Song>().ToList();

            // Intercept with AdvancedSearchParse 
            List<Song>? _advancedResult;
            List<Song> _songList = _currentSet;
            string? _searchBy = "Any";
            string? _searchAction = "SEARCH";

            // *** Advanced Search Parse ***
            LogTrace("===");
            if (AhLog._LoggingLevel.MinimumLevel < Serilog.Events.LogEventLevel.Debug)
                LogTrace($"Advanced Search by: '{_by}'   Action: '{_action}'   SearchText: '{_searchText}'");
            else
                LogMsg($"Search: \"{_searchText}\"");


            (_advancedResult, _searchBy, _searchAction) = AdvancedSearch(_currentSet, _searchText, _by, _action, _dbContext);
            if (_advancedResult != null)
            {
                _songList = _advancedResult.ToList();
                if (_searchBy != null) _by = _searchBy;
                if (_searchAction != null) _action = _searchAction;
            }

            if (_searchAction != "IS" && _advancedResult != null) RandomPersistentLogo(_searchText, _songList.Count);

            if (_songList.Count > 0)
            {
                await DispatchSonglist(_songList);
            }

            this.Dispatcher.Dispatch(() =>
            {
                if (_songList.Count > 0) SearchCount.Text = $"{_songList.Count:n0}";
                Searchby.SelectedItem = _searchBy;
                SearchAction.SelectedItem = _searchAction;
                var Placeholder = "Title, Artist, Band, Album, Genre, Path";
                if (_by != null && _action != null && (_by.ToLower() != "any" || _action.ToUpper() != "SEARCH")) Placeholder = $"SearchAction: {_searchAction}     -     SearchBy: {_searchBy}";
                _searchBar.Placeholder = Placeholder;
            });
        }



        private void Enable_Gui(bool _enable)
        {
            LogMsg($"* {(_enable ? "En" : "Dis")}abling Controls");
            var _opacity = _enable ? 1 : 0.25;
            TestPlaylist.IsEnabled = _enable;
            TestPlaylist.Opacity = _opacity;
            EditFrame.IsEnabled = _enable;
            EditFrame.Opacity = _opacity;
            StandardSearchBar.IsEnabled = _enable;
            StandardSearchBar.Opacity = _opacity;
            AdvancedSearchGrid.IsEnabled = _enable;
            AdvancedSearchGrid.Opacity = _opacity;
            MenuBox.IsEnabled = _enable;
            MenuBox.Opacity = _opacity;
        }


    }
}
