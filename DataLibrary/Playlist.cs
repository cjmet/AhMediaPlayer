using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLibrary
{
    // All the code in this file is included in all platforms.
    public class Playlist
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? LineItem { get => $"{Id} - {Name} - {Description}"; }
        public List<Song> Songs { get; set; }
    }
}
