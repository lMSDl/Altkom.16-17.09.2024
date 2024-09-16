namespace Models
{
    public class Order : Entity
    {
        public DateTime DateTime { get; set; }
        public string? Name { get; set; }
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}