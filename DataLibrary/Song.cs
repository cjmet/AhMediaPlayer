using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace DataLibrary
{
    public class Song
    {
        [Key]
        public int Id { get; set; }
        public string PathName { get; set; }
        public string FileName { get => Path.GetFileNameWithoutExtension(PathName); }
        public string Title { get; set; }
        public string? Comment { get; set; }
        public string LineItem { get => $"{Id} - {Title} - {Comment}"; }
        public string? Artist { get; set; }
        public string? Album { get; set; }
        public string? Genre { get; set; }
        public int? Year { get; set; }
        public string? Length { get; set; }
        public List<Playlist> Playlists;
    }
}

