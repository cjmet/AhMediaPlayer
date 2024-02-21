using System.ComponentModel.DataAnnotations;

namespace DataLibrary
{
    public class Song
    {
        [Key]
        public int Id { get; set; }
        public string PathName { get; set; }
        public string FileName { get => Path.GetFileNameWithoutExtension(PathName); }
        public string Title { get; set; }
        public string LineItem { get => $"{Title} - {Artist} - {Album}"; }
        public string? Artist { get; set; }
        public string? Band { get; set; }
        public string? Album { get; set; }
        public int? Track { get; set; }
        public int? Year { get; set; }
        public string? Genre { get; set; }
        public string? Length { get; set; }
        public List<Playlist> Playlists;
    }
}

