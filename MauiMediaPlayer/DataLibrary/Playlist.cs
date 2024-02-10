using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace DataLibrary
{
    // All the code in this file is included in all platforms.
    public class Playlist
    {

        [Key]
        public int Id;
        public string Name;
        public string Description;
        //public List<Song> Songs { get; set; }
    }
}
