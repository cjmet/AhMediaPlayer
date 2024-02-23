using AngelHornetLibrary;
using DataLibrary;
using Id3;
using static AngelHornetLibrary.AhLog;
using static DataLibrary.GenreDictionary;

namespace CommonNet8
{
    public static class SearchForMusic
    {

        public static async Task SearchUserProfileMusic()
        {
            var folder = Environment.SpecialFolder.UserProfile;
            var path = Environment.GetFolderPath(folder);
            var MusicPath = Path.Join(path, "Music");
            LogInfo($"SearchUserProfileMusic: {MusicPath}");

            await foreach (string filename in new AhGetFiles().GetFilesAsync(MusicPath, "*.mp3"))
            {
                LogInfo($"Adding[23]: {filename}");
                AddFilenameToSongDb(filename);
            }
            LogInfo("SearchUserProfileMusic Complete.");
            return;
        }

        public static void AddFilenameToSongDb(string filename)
        {
            var _adbContext = new PlaylistContext();
            {
                LogInfo($"*** Checking: {filename}");
                var _dirName = Path.GetDirectoryName(filename);
                var _fileName = Path.GetFileNameWithoutExtension(filename);
                if (_adbContext.Songs.Any(s => s.PathName == filename))
                {
                    LogInfo($"*** Already Exists: {filename}");
                    return;
                }
                LogInfo($"*** Adding[41]: {filename}");
                // Tag v2
                var tag = new Mp3(filename).GetTag(Id3TagFamily.Version2X);
                if (tag == null)
                {
                    LogInfo($"*** Tag v2 is null: {tag} -> {filename}");
                    tag = new Mp3(filename).GetTag(Id3TagFamily.Version1X);
                }
                // Tag v1
                if (tag == null)
                {
                    LogInfo($"*** Tag v1 is null: {tag} -> {filename}");
                    tag = new Mp3(filename).GetAllTags().FirstOrDefault();
                }
                // ANY Tag
                if (tag == null)
                {
                    LogInfo($"\n***\n*** ALL TAGS ARE NULL *** !!! {tag}\n     {filename}\n***\n");
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
                LogInfo($"*** Added: {filename}");
            }
            LogInfo("*** Saving Changes: _adbContext ***");
            _adbContext.SaveChanges();
            LogInfo("*** Changes Saved ***");
        }


    }
}
