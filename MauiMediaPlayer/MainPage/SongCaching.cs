using AhConfig;
using DataLibrary;
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






namespace MauiMediaPlayer
{
    public partial class MainPage : ContentPage
    {


        private async Task SongCache(Song _song, List<Song> _list, int _index)
        {
            // Cursory, Minimal, Check the Request
            // Is the SongCacheTask Running?
            // is the SongPlayTask Running?
            // clear the Song Queue
            // Signal the FileTransferTask
            // Reset contents of the Song Queue
            // Reset contents of the SongPlay Task

            // Quick Check
            LogDebug($"SongCache: {(_song != null ? _song.Title : "null")}");
            if (_song == null || _song.PathName == null)
            {
                LogDebug("Song is null");
                return;
            }




            // Clear the Queue
            _cacheSongQueue.Clear();
            _cacheSongQueue.Enqueue(null); // signal the queue to stop downloads

            // (Re)Fill the Queue, Queue it up fast, don't check the file systems, that just slows it all down.
            {
                for (int i = _index; i < _list.Count && i < _index + Const.CacheSize; i++)
                {
                    if (_list[i] == null) continue;
                    var _source = _list[i];
                    if (_source == null) continue;
                    if (_source == null || _source.PathName == null || _source.Title == null) continue;
                    LogTrace($" EnQueuing[{i - _index + 1}]: {_source.Title}");
                    _cacheSongQueue.Enqueue(_source);
                }
                LogDebug($" Queued: {_cacheSongQueue.Count} songs.");
            }

            // Signal the SongPlayTask
            _playSongStack.Push(_song);
        }

        private async Task<string> CopySongToCache(string _source, string _destination, long _fileSize, CancellationToken _token)
        {
            LogTrace($"Source: {_source}");
            LogTrace($"Destination: {_destination}");
            if (_source == null || _destination == null) return "";
            if (_token.IsCancellationRequested) return "";   /// smb over wan operations are slow, so check this often.

            if (!File.Exists(_source)) return "";
            if (_token.IsCancellationRequested) return "";

            // this should already be happening in a task, but it's lagging the GUI anyway
            // so lets wrap it in an 'async Task<long>' and see if that helps.
            // That helped, but It's still lagging the GUI.
            // Lets implement the isCached check in the caller to help this. We'll still have to call it once each, but should help.
            long _sourceSize;
            if (_fileSize > 0) _sourceSize = _fileSize;
            else
            {
                LogDebug($"  Getting File Size[293]: {_source}");
                _sourceSize = await Task.Run<long>(async () => { return new FileInfo(_source).Length; });
            }

            var _fileName = Path.GetFileNameWithoutExtension(_source);
            if (File.Exists(_destination) && new FileInfo(_destination).Length == _sourceSize)
            {
                LogDebug($"  Pre-Cached: {_fileName}");
                File.SetLastAccessTime(_destination, DateTime.Now);  /// Windows no longer updates this automatically.
                return _source;  // Pre-Cached
            }
            if (_token.IsCancellationRequested) return "";

            LogMsg($"  Caching: {_fileName}");
            try
            {
                // This can lag the UI up to 2 seconds when opening a file over the WAN.
                await using (FileStream sourceStream = File.OpenRead(_source))
                await using (FileStream destinationStream = File.Create(_destination))
                    await sourceStream.CopyToAsync(destinationStream, _token);

                // Lets try this to see if it's more responsive to the GUI. ... This is about the same, maybe a little more responsive, but less diagnostic.
                // Reference: await File.WriteAllBytesAsync(_destination, await File.ReadAllBytesAsync(_source, _token), _token);
            }
            catch (TaskCanceledException)
            {
                LogDebug($"  File Transfer Canceled");
                return "";
            }
            catch (Exception ex)
            {
                LogError($"CacheFile: {ex.Message}");
                return "";
            }
            LogTrace($"  Cached: {_fileName}");
            return _source;
        }

        private async Task CacheSongTask()
        {
            LogMsg($"SongCacheTask is Starting");
            LogDebug($"Song Cache Dir: {AppData.TmpDataPath}");
            var _song = new Song();
            var isCached = new List<string>();
            var _source = "";
            var _fileName = "";
            var _destination = "";
            var _cacheCleanDelay = 0;
            var _songTransferCTS = new CancellationTokenSource();
            var _copyTask = new Task<string>(() => { return null; });
            _copyTask.Start();

            while (true)
            {
                //if (_cacheSongQueue.TryDequeue(out _song))
                if (_cacheSongQueue.TryPeek(out _song))
                {
                    if (_song == null)
                    {
                        _cacheSongQueue.TryDequeue(out _song);  // eat the null/signal
                        while (!_cacheSongQueue.TryPeek(out _song) || _song == null) await Task.Delay(25);
                        if (_song != null && _song.PathName != null && _song.PathName == _source)
                        {
                            LogDebug($"  Primary Download Already in Progress: {_song.FileName}");
                            continue;  // we're already downloading the song we want next.
                        }
                        else if (_song != null && _song.PathName != null && isCached.Contains(_song.PathName))
                        {
                            LogDebug($"  Primary Pre-Cached: {_song.FileName}");
                            continue;  // we've already downloaded it.
                        }
                        // We are neither downloading, nor have we downloaded ... so cancel the current download and start the next one.
                        LogDebug($"Canceling File Transfer: {_fileName}");
                        _songTransferCTS.Cancel();
                        _songTransferCTS = new CancellationTokenSource();
                    }
                    else if (_copyTask.IsCanceled || _copyTask.IsCompleted || _copyTask.IsFaulted)
                    {
                        if (_copyTask.IsCompleted)
                        {
                            var _cached = _copyTask.Result;
                            if (_cached != null && _cached != "" && !isCached.Contains(_cached)) isCached.Add(_cached);
                        }
                        _cacheSongQueue.TryDequeue(out _song);
                        LogTrace($" DeQueuing[{_cacheSongQueue.Count + 1}]: {_song.Title}");

                        if (_song != null && _song.PathName != null && isCached.Contains(_song.PathName))
                        {
                            LogDebug($"  Secondary PreCached: {_song.FileName}");
                            continue;  // we've already downloaded it.
                        }

                        _songTransferCTS = new CancellationTokenSource();
                        var _token = _songTransferCTS.Token;
                        _source = _song.PathName;
                        _fileName = _song.FileName;
                        _destination = Path.Combine(AppData.TmpDataPath, Path.GetFileName(_song.PathName));
                        _copyTask = CopySongToCache(_source, _destination, _song.FileSize, _token);
                        if (_song != null) await Task.Delay(Const.ClockTick);  // Need the pause here for responsiveness.
                    }
                    else
                    {
                        LogTrace($"  Copying: {_fileName}");
                        await Task.Delay(Const.ClockTick);
                    }
                }
                else
                {
                    //LogTrace(" SongCacheTask is Waiting");
                    if (_cacheCleanDelay-- <= 0)
                    {
                        LogTrace("Cleaning Cache");
                        var _files = new DirectoryInfo(AppData.TmpDataPath)
                       .EnumerateFiles()
                       .Where(f => f.Extension != ".db" && f.Extension != ".db-shm" && f.Extension != ".db-wal")
                       .OrderByDescending(f => f.LastAccessTime);
                        var _fileCount = 0;
                        foreach (var _file in _files.Skip(Const.CacheSizeMax))
                        {
                            LogTrace($"Un-Caching: [{_file.LastAccessTime}] {_file.Name}");
                            _file.Delete();
                            _fileCount++;
                        }
                        LogTrace($"Cleaning Cache: {_fileCount} files removed.");
                        _cacheCleanDelay = Const.CacheCleanInterval;
                    }
                    await Task.Delay(Const.ClockTick);
                }
            }
        }

        private async Task PlaySongTask()
        {
            LogMsg("SongPlayTask is Starting");
            var _song = new Song();
            var _source = "";
            long _sourceSize = 0;
            var _destination = "";
            var spinner = 0;
            while (true)
            {
                if (_playSongStack.TryPop(out Song _tmp))
                {
                    string poppedTitle;
                    if (_tmp != null && _tmp.Title != null) poppedTitle = _tmp.Title;
                    else poppedTitle = "null";
                    LogDebug($"  Popping[{_playSongStack.Count + 1}]: {poppedTitle}");
                    if (_tmp == null) continue;
                    _song = _tmp;
                    _source = _song.PathName;
                    if (_song.FileSize > 0) _sourceSize = _song.FileSize;
                    else _sourceSize = 0;
                    _destination = Path.Combine(AppData.TmpDataPath, Path.GetFileName(_song.PathName));
                    _playSongStack.Clear();
                }
                else if (File.Exists(_destination) && new FileInfo(_destination).Length > 8)
                {
                    var _destinationSize = new FileInfo(_destination).Length;

                    if (_sourceSize == 0)
                    {
                        LogDebug($"  Getting File Size[455]: {_source}");
                        _sourceSize = await Task.Run<long>(async () => { return new FileInfo(_source).Length; });  // lets only do this once.
                    }
                    if (_sourceSize == _destinationSize)
                    {
                        PlaySong(_song, _destination);
                        _source = _destination = "";
                    }
                    else
                    {
                        double _percent = (double)_destinationSize / _sourceSize;
                        var denom = Const.INF / Const.ClockTick;
                        if (spinner++ % denom == 0)
                            LogMsg($"  Loading[{_percent:P0}]: {Path.GetFileNameWithoutExtension(_source)}");
                        await Task.Delay(Const.ClockTick);
                    }
                }
                else
                {
                    //LogTrace("  SongPlayTask is Waiting"); 
                    await Task.Delay(Const.ClockTick);
                }
            }

        }

    }
}
