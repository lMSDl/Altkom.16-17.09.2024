using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class Order : Entity
    {
        private DateTime dateTime;
        private string? alamakota;

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
        public string? Name
        {
            get => alamakota;
            set
            {
                alamakota = value;
            }
        }
        public ICollection<Product> Products { get; set; } = new ObservableCollection<Product>();


        public string? Description { get; }
    }


}