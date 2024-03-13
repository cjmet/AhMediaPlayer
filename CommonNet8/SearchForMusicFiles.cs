using AngelHornetLibrary;
using DataLibrary;
using Id3;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using static AngelHornetLibrary.AhLog;
using static DataLibrary.GenreDictionary;
using AhConfig;



namespace CommonNet8
{

    public class ReportProgressToQueue : IProgress<string>
    {
        public ConcurrentQueue<string> _queue = new ConcurrentQueue<string>();
        static int spinner = 0;
        public ReportProgressToQueue(ConcurrentQueue<string> targetQueue)
        {
            _queue = targetQueue;
        }
        public void Report(string value)
        {
            if (spinner++ % 30 == 0)
            {
                LogInfo($"Srch: {value}");
            }
        }
    }


    public class ReportByAction : IProgress<string>
    {
        Action<string> _logFunction;
        static int spinner = 0;
        public ReportByAction(Action<string> function)
        {
            _logFunction = function;
        }
        public void Report(string value)
        {
            _logFunction(value);
        }
    }


    public static class SearchForMusicFiles
    {

        public static async Task SearchUserProfileMusic(PlaylistContext _dbContext, IProgress<string> progress, string SearchPath = "")
        {
            LogMsg($"Searching for Music");
            var folder = Environment.SpecialFolder.UserProfile;
            var path = Environment.GetFolderPath(folder);
            List<string> MusicPaths= new List<string>();
            if (SearchPath != "") MusicPaths.Add(SearchPath);
            else MusicPaths.Add(Path.Join(path, "Music"));
            foreach (var _path in MusicPaths) LogMsg($"   Path: {_path}");

            List<string> _filesCache = new List<string>();
            _filesCache = await _dbContext.Songs.Select(s => s.PathName).ToListAsync();

            await foreach (string filename in new AhGetFiles().GetFilesAsync(MusicPaths, "*.mp3", iprogress: progress))
            {
                if (_filesCache.Contains(filename))
                {
                    LogTrace($"Exists[23]: {filename}");
                    continue;
                }
                else
                {
                    LogTrace($"Adding[23]: {filename}");
                    await AddFilenameToSongDb(filename, _dbContext);
                    await Task.Delay(1);                    // We need this delay for GUI Responsiveness.  1ms minimum, 10ms on Gb Lan, 40ms is 25fps, 100ms 100mb LAN, 1000ms 10mb LAN
                }
            }
            LogMsg("Search for Music Files Complete.");
            return;
        }

        public static async Task AddFilenameToSongDb(string filename, PlaylistContext _adbContext)
        {

            {
                LogTrace($"*** Checking: {filename}");
                var _dirName = Path.GetDirectoryName(filename);
                var _fileName = Path.GetFileNameWithoutExtension(filename);
                if (_adbContext.Songs.Any(s => s.PathName == filename))
                {
                    LogTrace($"*** Already Exists: {filename}");
                    return;
                }
                LogTrace($"*** Adding[41]: {filename}");

                Id3Tag? tag = null;






                var isolatedIOTask = new Task( () =>              // cj - Isolating this further into it's own task, to see if this helps GUI responsiveness
                {
                    LogTrace($"Reading Tags[88]:  {filename}");
                    // Try Catch all the Id3TagFamily for malformed and corrupted tags
                    {
                        try { tag = new Mp3(filename).GetTag(Id3TagFamily.Version2X); }
                        catch (Exception ex) { tag = null; LogTrace($"*** Tag v2 Exception: {ex.Message}"); }
                    }
                    LogTrace($"Reading Tags[94]");
                    if (tag == null)
                    {
                        //await Task.Delay(1);
                        try { tag = new Mp3(filename).GetTag(Id3TagFamily.Version1X); }
                        catch (Exception ex) { tag = null; LogTrace($"*** Tag v1 Exception: {ex.Message}"); }
                    }
                    LogTrace($"Reading Tags[101]");
                    if (tag == null)
                    {
                        //await Task.Delay(1);
                        try { tag = new Mp3(filename).GetAllTags().FirstOrDefault(); }
                        catch (Exception ex) { tag = null; LogTrace($"*** Tag Any Exception: {ex.Message}"); }
                    }
                    // /Get Tag
                    LogTrace($"Tags Read:[109] {(tag != null ? tag : "null")}");
                    //await Task.Delay(1);
                }, TaskCreationOptions.LongRunning);
                isolatedIOTask.Start();
                await isolatedIOTask;
                LogTrace($"Tags Read Task Complete[114]: {(tag != null ? tag : "null")}");


                if (tag == null)
                {
                    LogTrace($"All Tags are Null! [{tag}] {_fileName}");
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
                        if (tag.Genre.ToString() != "")
                        {
                            LogTrace($"*** Genre: [{tag.Genre}]");
                            _genre = GenreLookup(tag.Genre);
                        }

                // Fix Corrupted Tags
                LogTrace($"*** Fix Corrupted Tags: {(tag != null ? tag : "null")}");
                if (tag.Title == null || tag.Title.ToString() == null || tag.Title.ToString() == "") tag.Title = _fileName != null ? _fileName : "null";

                LogTrace("Fix FileSize for Wan and FileInfo Lag Issues");
                long _fileSize = new FileInfo(filename).Length;                 // we want to ask this question only once if possible.

                LogTrace("MP3 Trace Data");
                LogTrace($"Pa:[{filename}]");
                LogTrace($"Fs:[{_fileSize}]");
                LogTrace($"Fi:[{_fileName}]");
                LogTrace($"Ti:[{tag.Title}]");
                LogTrace($"Ar:[{tag.Artists}]");
                LogTrace($"Ba:[{tag.Band}]");
                LogTrace($"Al:[{tag.Album}]");
                LogTrace($"Tr:[{tag.Track}]");
                LogTrace($"Gn:[{tag.Genre}]");
                LogTrace($"Yr:[{tag.Year}]");
                LogTrace($"Ln:[{tag.Length}]");

                Song _newSong = new Song();
                _newSong.PathName = filename;
                _newSong.FileSize = _fileSize;
                _newSong.Title = tag.Title;
                _newSong.Artist = tag.Artists;
                _newSong.Band = tag.Band;
                _newSong.Album = tag.Album;
                _newSong.Track = tag.Track;
                _newSong.Genre = _genre;
                _newSong.Year = tag.Year;
                _newSong.Length = tag.Length;

                _adbContext.Songs.Add(_newSong);
                // there does not appear to be a tag.Dispose() method
                LogTrace($"*** Added: {filename}");
            }
            LogTrace("*** Saving Changes: _adbContext ***");
            _adbContext.SaveChanges();
            LogTrace("*** Changes Saved ***");
        }


    }
}
