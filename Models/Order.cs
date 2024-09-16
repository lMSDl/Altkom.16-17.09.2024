using System.Collections.ObjectModel;

namespace Models
{
    public class Order : Entity
    {
        private DateTime dateTime;

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