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
        //public Playlist() : this("null", null) { }
        //public Playlist(string name) : this(name, null) { }
        //public Playlist(string name, string description)
        //public Playlist
        //{
        //    Name = name;
        //    Description = description;
        //}


        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        
        //public List<Song> Songs { get; set; }
    }
}
