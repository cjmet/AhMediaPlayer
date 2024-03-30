using System.Collections;

namespace DataLibrary
{
    // All the code in this file is included in all platforms.
    public class Playlist : IList<Song>
    {
        public Song this[int index] { get => ((IList<Song>)Songs)[index]; set => ((IList<Song>)Songs)[index] = value; }

        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<Song> Songs { get; set; } = new List<Song>();
        public List<int> SongIds { get; set; } = new List<int> {};
        public string? LineItem { get => $"{Id} - {Name} - {Description}"; }

        public int Count => Songs.Count;
        public bool IsReadOnly => false;

        public void Add(Song item)
        {
            Songs.Add(item);
            SongIds.Add(item.Id);
        }

        public void Clear()
        {
            Songs.Clear();
            SongIds.Clear();
        }

        public bool Contains(Song item)
        {
            return Songs.Contains(item);
        }

        public void CopyTo(Song[] array, int arrayIndex)
        {
            Songs.CopyTo(array, arrayIndex);
        }

        public void Insert(int index, Song item)
        {
            Songs.Insert(index, item);
        }

        public bool Remove(Song item)
        {
            var results = Songs.Remove(item);
            if (results) SongIds.Remove(item.Id);
            return results;
        }

        public void RemoveAt(int index)
        {
            Songs.RemoveAt(index);
            SongIds.RemoveAt(index);
        }

        public int IndexOf(Song item)
        {
            return Songs.IndexOf(item);
        }

        public IEnumerator<Song> GetEnumerator()
        {
            return ((IEnumerable<Song>)Songs).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)Songs).GetEnumerator();
        }
    }


}
