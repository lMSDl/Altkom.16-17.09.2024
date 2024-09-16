using System.Collections.ObjectModel;

namespace Models
{
    public class Order : Entity
    {
        public virtual DateTime DateTime { get; set; }
        public virtual string? Name { get; set; }
        public virtual ICollection<Product> Products { get; set; } = new ObservableCollection<Product>();
    }
}