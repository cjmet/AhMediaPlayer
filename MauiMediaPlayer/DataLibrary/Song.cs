using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace DataLibrary
{
    public class Song
    {
        [Key]
        public int Id;
        public string FileName { get; set; }
        //public List<Playlist> Playlists;
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public List<string> Genres { get; set; }
        public int Year { get; set; }
        public string Length { get; set; }
        public string Comment { get; set; }
    }
}
