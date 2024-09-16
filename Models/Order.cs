using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class Order : Entity
    {
        private DateTime dateTime;

        //odpowiednik IsConcurencyToken w konfiguracji
        //[ConcurrencyCheck]
        public DateTime DateTime
        {
            get => dateTime;
            set
            {
                dateTime = value;
                OnPropertyChanged();
            }
        }
        public string? Name { get; set; }
        public ICollection<Product> Products { get; set; } = new ObservableCollection<Product>();
    }
}