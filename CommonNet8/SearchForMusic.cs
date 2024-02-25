﻿using AngelHornetLibrary;
using DataLibrary;
using Id3;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Diagnostics;
using static AngelHornetLibrary.AhLog;
using static DataLibrary.GenreDictionary;
using System.Text.RegularExpressions;

namespace CommonNet8
{

    public class ReportProgressToQueue : IProgress<string>
    {
        public ConcurrentQueue<string> _queue = new ConcurrentQueue<string>();
        public ReportProgressToQueue(ConcurrentQueue<string> targetQueue)
        {
            _queue = targetQueue;
        }
        public void Report(string value)
        {
            LogMsg(value);
        }
    }


    public static class SearchForMusic
    {

        public static async Task SearchUserProfileMusic(IProgress<string> progress)
        {
            var folder = Environment.SpecialFolder.UserProfile;
            var path = Environment.GetFolderPath(folder);
            var MusicPath = Path.Join(path, "Music");
            LogMsg($"Searching UserProfile/Music");
            MusicPath = "M:\\";
            LogMsg($"{MusicPath}");
            List<string> _filesCache = new List<string>();
            var _dbContext = new PlaylistContext();
            _filesCache = await _dbContext.Songs.Select(s => s.PathName).ToListAsync();

            await foreach (string filename in new AhGetFiles().GetFilesAsync(MusicPath, "*.mp3", iprogress: progress))
            {
                if (_filesCache.Contains(filename))
                {
                    LogTrace($"Exists[23]: {filename}");
                    continue;
                }
                else
                {
                    LogTrace($"Adding[23]: {filename}");
                    AddFilenameToSongDb(filename);
                    //await Task.Delay(1);
                }
            }
            LogMsg("SearchUserProfileMusic Complete.");
            return;
        }

        public static void AddFilenameToSongDb(string filename)
        {
            var _adbContext = new PlaylistContext();
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

                // Try Catch all the Id3TagFamily for malformed and corrupted tags
                {
                    try { tag = new Mp3(filename).GetTag(Id3TagFamily.Version2X); }
                    catch (Exception ex) { tag = null; LogTrace($"*** Tag v2 Exception: {ex.Message}"); }
                }
                if (tag == null)
                {
                    try { tag = new Mp3(filename).GetTag(Id3TagFamily.Version1X); }
                    catch (Exception ex) { tag = null; LogTrace($"*** Tag v1 Exception: {ex.Message}"); }
                }
                if (tag == null)
                {
                    try { tag = new Mp3(filename).GetAllTags().FirstOrDefault(); }
                    catch (Exception ex) { tag = null; LogTrace($"*** Tag Any Exception: {ex.Message}"); }
                }
                // /Get Tag

                if (tag == null)
                {
                    LogTrace($"All Tags are Null! [{tag}] {_fileName}");
                    tag = new Mp3(filename).GetTag(Id3TagFamily.Version1X);
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
                LogTrace($"*** Fix Corrupted Tags");
                if (tag.Title.ToString() == null || tag.Title.ToString() == "") tag.Title = _fileName;

                LogTrace("MP3 Trace Data");
                LogTrace($"Pa:[{filename}]");
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
