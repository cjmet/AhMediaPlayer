using System.Collections.Concurrent;
using DataLibrary;
using System.Diagnostics;
using Id3;
using static DataLibrary.GenreDictionary;
using AngelHornetLibrary;

namespace CommonNet8
{
    public static class SearchForMusic
    {

        public static async Task SearchUserProfileMusic()
        {
            var folder = Environment.SpecialFolder.UserProfile;
            var path = Environment.GetFolderPath(folder);
            var MusicPath = Path.Join(path, "Music");
            Debug.WriteLine($"MusicPath: {MusicPath}");

            await foreach (string filename in new AhGetFiles().GetFilesAsync(MusicPath, "*.mp3"))
            {
                Debug.WriteLine($"   Adding: {filename}");
                AddFilenameToSongDb(filename);
            }
            return;
        }

        public static void AddFilenameToSongDb(string filename)
        {
            var _adbContext = new PlaylistContext();
            {
                Debug.WriteLine($"*** Adding: {filename}");
                var _dirName = Path.GetDirectoryName(filename);
                var _fileName = Path.GetFileNameWithoutExtension(filename);
                if (_adbContext.Songs.Any(s => s.PathName == filename))
                {
                    Debug.WriteLine($"*** Already Exists: {filename}");
                    return;
                }
                Debug.WriteLine($"*** Adding: {filename}");
                // Tag v2
                var tag = new Mp3(filename).GetTag(Id3TagFamily.Version2X);
                if (tag == null)
                {
                    Debug.WriteLine($"*** Tag v2 is null: {tag} -> {filename}");
                    tag = new Mp3(filename).GetTag(Id3TagFamily.Version1X);
                }
                // Tag v1
                if (tag == null)
                {
                    Debug.WriteLine($"*** Tag v1 is null: {tag} -> {filename}");
                    tag = new Mp3(filename).GetAllTags().FirstOrDefault();
                }
                // ANY Tag
                if (tag == null)
                {
                    Debug.WriteLine($"\n***\n*** ALL TAGS ARE NULL *** !!! {tag}\n     {filename}\n***\n");
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
                        _genre = GenreLookup(tag.Genre);

                _adbContext.Songs.Add(new Song
                {
                    PathName = filename,
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
                Debug.WriteLine($"*** Added: {filename}");
            }
            Debug.WriteLine(" *** Saving Changes: _adbContext ***");
            _adbContext.SaveChanges();
            Debug.WriteLine(" *** Changes Saved ***");
        }


    }
}
