﻿using System.ComponentModel.DataAnnotations;
using System.Runtime;
using System.Text.RegularExpressions;

namespace DataLibrary
{
    public class Song
    {
        [Key]
        public int Id { get; set; }
        public string PathName { get; set; } = "";
        public long FileSize { get; set; }

        public string Title
        {
            get => title;
            set
            {
                title = value;
                AlphaTitle= CreateSongAlphaTitle(value);
            }
        }
        public string AlphaTitle { get; private set; } = "";  // Cleaned up AlphaNumeric Only, Alphabetical Title for Sorting
        public string? Artist { get; set; }
        public string? Band { get; set; }
        public string? Album { get; set; }
        public int? Track { get; set; }
        public int? Year { get; set; }
        public string? Genre { get; set; }
        public string? Length { get; set; }
        public List<Playlist> Playlists;
        private string title;

        public string FileName { get => Path.GetFileNameWithoutExtension(PathName); }
        public string LineItem { get => $"{Title} {Artist} {Band} {Album} {Genre}"; }
        public bool Star { get; set; } = false;



        public static string CreateSongAlphaTitle(string title)
        {
            var throwback = title;
            // Clean up anything that isn't alphanumeric or a space
            title = Regex.Replace(title, @"[^\w ]", "", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            // Remove double spaces
            title = Regex.Replace(title, @"\s+", " ", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            // Remove leading spaces etc
            title = Regex.Replace(title, @"^[^a-z0-9]*", "", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var fallback = title;
            // Remove leading numbers etc
            title = Regex.Replace(title, @"^[^a-z]*", "", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            // Remove leading "The" and space
            title = Regex.Replace(title, @"^The\s+", "", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            // Remove leading "A" and space
            title = Regex.Replace(title, @"^A\s+", "", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            if (title.Length > 0) return title;
            else if (fallback.Length > 0) return fallback;
            else return throwback;
        }

    }

}

