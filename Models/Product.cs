using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Models
{
    public class Product : Entity
    {
        private ILazyLoader _lazyLoader;
        private Order? order;

        public Product() { }
        public Product(ILazyLoader lazyLoader)
        {
            _lazyLoader = lazyLoader;
        }

        public string Name { get; set; } = string.Empty;
        public float Price { get; set; }
        public Order? Order
        {
            get
            {
                if (order == null)
                {
                    try
                    {
                        _lazyLoader.Load(this, ref order);
                    }
                    catch
                    {
                        order = null;
                    }
                }
                return order;
            }
            set => order = value;
        }

        //public int? OrderId { get; set; }

        //odpowiednik IsRowVersion w konfiguracji
        //[Timestamp]
        //public byte[] Timestamp { get; } //przeniesione do ShadowProperty

        public ProductDetails? Details { get; set; }

    }
}
