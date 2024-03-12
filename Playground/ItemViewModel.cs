using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playground
{
    public class ItemViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Item> Items { get; set; } = new ObservableCollection<Item>();

        public event PropertyChangedEventHandler PropertyChanged;

        public ItemViewModel()
        {
            // Add sample data
            Items.Add(new Item { Id = 1, Name = "Item 1" });
            Items.Add(new Item { Id = 2, Name = "Item 2" });
            Items.Add(new Item { Id = 3, Name = "Item 3" });
        }

        // Add an item
        public void AddItem(string name)
        {
            int id = Items.Count + 1;
            Items.Add(new Item { Id = id, Name = name });
        }

        // Update an item
        public void UpdateCount(int id)
        {
            var item = Items.FirstOrDefault(i => i.Id == id);
            if (item != null)
            {
                item.Count++;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Items)));
            }
        }


        // Update an item
        public void UpdateItem(int id, string name)
        {
            var item = Items.FirstOrDefault(i => i.Id == id);
            if (item != null)
            {
                item.Name = name;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(item)));
            }
        }

        // Delete an item
        public void DeleteItem(int id)
        {
            var item = Items.FirstOrDefault(i => i.Id == id);
            if (item != null)
            {
                Items.Remove(item);
            }
        }
    }
}
