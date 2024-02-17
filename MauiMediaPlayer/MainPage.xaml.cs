using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AngelHornetLibrary.CLI;
using DataLibrary;
using AngelHornetLibrary;
using CommunityToolkit.Maui.Views;
using Id3;
using static MauiMediaPlayer.ProgramLogic.GenreDictionary;
using MauiMediaPlayer.ProgramLogic;
using System.ComponentModel;
using System.Windows.Input;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Numerics;
using Microsoft.EntityFrameworkCore;
using WinRT;
using Microsoft.Maui.Controls.Internals;




namespace MauiMediaPlayer
{
    public partial class MainPage : ContentPage
    {

        int count = 10;

        private readonly PlaylistContext _dbContext;
        List<Song> _mainSongList = new List<Song>();


        public MainPage(PlaylistContext dbcontext)
        //public MainPage()
        {
            InitializeComponent();
            _dbContext = dbcontext;

            //=== ======================================
            //vvv vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv

            // Callback for FindFilesConcurrentQueue
            Action<ConcurrentQueue<string>, bool> callback = (ConcurrentQueue<string> q, bool doneFlag) =>
            {
                string r;

                var _adbContext = new PlaylistContext();
                while (q.TryDequeue(out r))
                {
                    Debug.WriteLine($"*** DeQueueing: {r}");
                    var _dirName = Path.GetDirectoryName(r);
                    var _fileName = Path.GetFileNameWithoutExtension(r);
                    Debug.WriteLine($"*** Adding: {r}");
                    // Tag v2
                    var tag = new Mp3(r).GetTag(Id3TagFamily.Version2X);
                    if (tag == null)
                    {
                        Debug.WriteLine($"*** Tag v2 is null: {tag} -> {r}");
                        tag = new Mp3(r).GetTag(Id3TagFamily.Version1X);
                    }
                    // Tag v1
                    if (tag == null)
                    {
                        Debug.WriteLine($"*** Tag v1 is null: {tag} -> {r}");
                        tag = new Mp3(r).GetAllTags().FirstOrDefault();
                    }
                    // ANY Tag
                    if (tag == null)
                    {
                        Debug.WriteLine($"\n***\n*** ALL TAGS ARE NULL *** !!! {tag}\n     {r}\n***\n");
                        tag = new Mp3(r).GetTag(Id3TagFamily.Version1X);
                        tag = new Id3Tag
                        {
                            Title = _fileName,
                            Artists = new Id3.Frames.ArtistsFrame(),
                            Band = "",
                            Album = "",
                            Track = 0,
                            Genre = "",
                            Year = 0,
                            Length = new Id3.Frames.LengthFrame(),
                        };
                    }

                    string _genre = "";
                    if (tag != null)
                        if (tag.Genre != null)
                            _genre = GenreLookup(tag.Genre);

                    _adbContext.Songs.Add(new Song
                    {
                        PathName = r,
                        Title = tag.Title,
                        Artist = tag.Artists,
                        Band = tag.Band,
                        Album = tag.Album,
                        Track = tag.Track,
                        Genre = _genre,
                        Year = tag.Year,
                        Length = tag.Length,
                    });
                    // there does not appear to be a tag.Dispose() method
                    Debug.WriteLine($"*** Added: {r}");
                }
                Debug.WriteLine(" *** Saving Changes: _adbContext ***");
                _adbContext.SaveChanges();
                Debug.WriteLine(" *** Saved ***");

                if (_adbContext.Songs.Count() > 0 || doneFlag)
                {

                    Debug.WriteLine(" *** _adbContext.Songs.Count() > 0 ***");
                    // cjm - StringComparer.OrdinalIgnoreCase is causing a crash here
                    // changed to query into a tmp List, then sort the tmp List, then set the ItemSource
                    //_adbContext.Songs.OrderBy(s => s.Title));   
                    Debug.WriteLine(" *** Dispatch Playlists to UI");
                    // cjm - Add Code Here Later, if needed, and if this is the right place for it?
                    Debug.WriteLine(" *** Doh, there are none, because I cleared them out.  Rewrite this later and insert it.");
                    // ---
                    Debug.WriteLine(" *** Dispatch Songs to UI");
                    var _songs = _adbContext.Songs.ToList();
                    _songs = _songs.OrderBy(s => s.Title, StringComparer.OrdinalIgnoreCase).ToList();
                    Application.Current.MainPage.Dispatcher.Dispatch(() =>
                        TestSonglist.ItemsSource = _songs);

                    Application.Current.MainPage.Dispatcher.Dispatch(() =>
                        messageText.Text = $"{_adbContext.Songs.Count().ToString()} Songs Found");

                    // vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv

                    if (doneFlag)
                    {
                        _songs = _adbContext.Songs.ToList();
                        _songs = _songs.OrderBy(s => s.Title, StringComparer.OrdinalIgnoreCase).ToList();
                        var _song = _songs.FirstOrDefault();

                        Debug.WriteLine("\n=== ======================================== ===\n");
                        Debug.WriteLine($"[138] Attempting to set _mediaSource = {_song.PathName}");
                        Debug.WriteLine("\n=== ======================================== ===\n");
                        MediaSource _mediaSource = null;
                        // cjm - this is causing a crash here
                        if (_song.PathName != null)
                        {
                            _mediaSource = null;
                            if (File.Exists(_song.PathName)) _mediaSource = MediaSource.FromFile(_song.PathName);
                            else if (_song.PathName.StartsWith("embed://")) _mediaSource = MediaSource.FromResource(_song.PathName.Substring(8));
                            Debug.WriteLine($"_mediaSource = {(_mediaSource == null ? "null" : _mediaSource.ToString())}");
                            if (_mediaSource != null)
                            {
                                mediaElement.Source = _mediaSource;
                                Application.Current.MainPage.Dispatcher.Dispatch(() =>
                                    mediaTitle.Text = _song.Title);
                                Application.Current.MainPage.Dispatcher.Dispatch(() =>
                                    mediaArtist.Text = $"{_song.Artist} - {_song.Album}");
                            }
                        }
                        // /cjm - this is causing a crash here
                    }
                    // ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
                }
                //_adbContext.Dispose();  // cjm - this is causing a crash ... so don't dispose of it, let the program do it for me?
            };
            // /Callback for FindFilesConcurrentQueue

            {
                List<string> paths = new List<string> { "C:\\users\\cjmetcalfe\\Music", "c:\\users\\khaai\\Music" };
                Debug.WriteLine("\n=== ======================================== ===\n");
                Debug.WriteLine("FindFilesConcurrentQueue ... ");
                foreach (var _path in paths)
                    Debug.WriteLine($"::> {_path}");
                Debug.WriteLine("\n=== ======================================== ===\n");

                new FindFilesConcurrentQueue(callback, paths, "*.mp3", SearchOption.AllDirectories);
            }

            //^^^ ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            //=== ======================================



            TestPlaylist.ItemsSource = _dbContext.Playlists.ToList();
            _mainSongList = _dbContext.Songs.ToList();
            _mainSongList = _mainSongList.OrderBy(s => s.Title, StringComparer.OrdinalIgnoreCase).ToList();
            TestSonglist.ItemsSource = _mainSongList;


            // ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            // ==========================================================
            // Test Background Tasks .... 


            //Task.Run(() =>
            //{
            //    while (true)
            //    {
            //        Task.Delay(1000).Wait();
            //        count++;
            //        Application.Current.MainPage.Dispatcher.Dispatch(() => counterText.Text = count.ToString()); //  String.Format("Downloading {0}%", count)); 
            //    }
            //});


            //    Task task;
            //    //List<string> targetDirs = ["C:\\users\\cjmetcalfe\\Music"];
            //    List<string> targetDirs = ["C:\\users\\cjmetcalfe\\music", "c:\\users\\khaai\\music",];
            //    task = new Task(() =>
            //    {
            //        foreach (var dir in targetDirs)
            //        {
            //            Debug.WriteLine($"\n*** Searching *** \n::> {dir}\n");
            //            new AhsUtil().GetFilesRef(dir, "*.mp3", ref result, ref searchStatus, SearchOption.AllDirectories);
            //        }
            //    }, TaskCreationOptions.LongRunning);
            //    task.Start();


            //    Task.Run(() =>
            //    {
            //        var spinner = 0;
            //        do
            //        {
            //            //string tmp = searchStatus;
            //            //if (tmp == null) tmp = "null";
            //            var tmp = $"{searchStatus}";
            //            var maxLength = 60;
            //            if (tmp.Length > maxLength) tmp = "" + tmp.Substring(tmp.Length - maxLength, maxLength);
            //            var spin = "|/-\\"[spinner++ % 4];
            //            tmp = $"[{result.Count}] {spin} {tmp}";
            //            Application.Current.MainPage.Dispatcher.Dispatch(() => pathText.Text = tmp);
            //            Task.Delay(250).Wait();
            //        } while (task.Status == TaskStatus.Running);
            //        Application.Current.MainPage.Dispatcher.Dispatch(() => pathText.Text = $"Found [{result.Count}] Mp3 files.");
            //    });



            // ==========================================================
            // vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv
        }

        private void TestSonglist_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            Debug.WriteLine("TestSonglist_ItemSelected");
            Debug.WriteLine("Sender: " + sender.ToString());
            Debug.WriteLine("Selected: " + e.SelectedItem);
            Debug.WriteLine("SelectedIndex: " + e.SelectedItemIndex);
            var _song = (Song)e.SelectedItem;
            var _index = e.SelectedItemIndex;
            var _listView = (ListView)sender;
            _listView.Focus();
            TestSonglist.SelectedItem = _song;

            Debug.WriteLine($"[262] Attempting to set _mediaSource = {_song.PathName}");
            MediaSource _mediaSource;

            if (_song.PathName != null)
            {
                _mediaSource = null;
                if (File.Exists(_song.PathName)) _mediaSource = MediaSource.FromFile(_song.PathName);
                else if (_song.PathName.StartsWith("embed://")) _mediaSource = MediaSource.FromResource(_song.PathName.Substring(8));
                Debug.WriteLine($"_mediaSource = {(_mediaSource == null ? "null" : _mediaSource.ToString())}");
                if (_mediaSource != null)
                {
                    mediaElement.ShouldAutoPlay = true;
                    mediaTitle.Text = _song.Title;
                    mediaArtist.Text = $"{_song.Artist} - {_song.Album}";
                    mediaElement.Source = _mediaSource;
                    //Task.Run(() =>
                    //{
                    //    Task.Delay(1000).Wait();
                    //    mediaElement.ShouldAutoPlay = true;
                    //    mediaElement.Play();
                    //}); // cjm - fix this later with a proper verify if media is loaded, then play it
                    //mediaElement.Play();
                    //Application.Current.MainPage.Dispatcher.Dispatch(() =>
                    //           mediaElement.Source = _mediaSource);
                    //var results = Application.Current.MainPage.Dispatcher.Dispatch(() =>
                    //                              mediaElement.Play());
                    //Debug.WriteLine($"mediaElement.Play() = {results}");
                    //Application.Current.MainPage.Dispatcher.Dispatch(() =>
                    //                       mediaTitle.Text = _song.Title);
                    //Application.Current.MainPage.Dispatcher.Dispatch(() =>
                    //                       mediaArtist.Text = $"{_song.Artist} - {_song.Album}");

                }
            }
        }

        private void mediaElement_MediaEnded(object sender, EventArgs e)
        {
            Debug.WriteLine("MediaElement_MediaEnded");
            Debug.WriteLine("   Sender: " + sender.ToString());
            Debug.WriteLine("   EventArgs_String: " + e.ToString);
            Debug.WriteLine("   EventArgs_Type: " + e.GetType);

            var _selectedSong = (Song)TestSonglist.SelectedItem;
            var _sourceSongList = TestSonglist.ItemsSource.Cast<Song>().ToList();
            //_selectedSong = _mainSongList.Where(s => s.Title == _selectedSong.Title).FirstOrDefault();
            var _selectedSongIndex = _sourceSongList.IndexOf(_selectedSong);
            var _newSong = _sourceSongList.ElementAtOrDefault(_selectedSongIndex + 1);
            var _newSongIndex = _sourceSongList.IndexOf(_newSong);

            Debug.WriteLine($"Changing Songs: ");
            Debug.WriteLine($"   From[{_selectedSongIndex}]: {_selectedSong.Title}");
            if (_newSong == null)
            {
                Debug.WriteLine($"     To[{_newSongIndex}]: null");
                return;
            }
            Debug.WriteLine($"     To[{_newSongIndex}]: {_newSong.Title}");

            if (_newSong.PathName != null)
            {
                MediaSource _mediaSource = null;
                if (File.Exists(_newSong.PathName)) _mediaSource = MediaSource.FromFile(_newSong.PathName);
                else if (_newSong.PathName.StartsWith("embed://")) _mediaSource = MediaSource.FromResource(_newSong.PathName.Substring(8));
                if (_mediaSource != null)
                    Task.Run(() =>
                    {
                        Debug.WriteLine($"[314]  TestSonglist.SelectedItem = _newSong ... ");
                        Application.Current.MainPage.Dispatcher.Dispatch(() =>  // This *** DOES *** need to be Dispatched.
                                 TestSonglist.SelectedItem = _newSong);                  // This triggers our select event ... so don't sent anythng else here, let the event handle it.
                        Debug.WriteLine($"[316]  Scrolling ... ");
                        Application.Current.MainPage.Dispatcher.Dispatch(() =>
                              TestSonglist.ScrollTo(_newSong, ScrollToPosition.Start, true));
                        Debug.WriteLine($"[316]  Selection Complete");

                    }
                    );
            }
        }
    }


    //public class MediaElementCommands
    //{
    //    public ICommand PlayOneSong { get; private set; }
    //    public MediaElementCommands()
    //    {
    //        PlayOneSong = new Command((p) => { Debug.WriteLine($"PlayOneSong: {p}"); });
    //    }
    //}

}
