using DAL;
using Microsoft.EntityFrameworkCore;
using Models;

var configurationOptions = new DbContextOptionsBuilder<Context>()
    .UseSqlServer(@"Server=(local);Database=EfCore;Integrated security=true;TrustServerCertificate=true")
    //.LogTo(Console.WriteLine)
    .Options;


var order = new Order();
var product = new Product() { Name = "marchewka", Price = 15};

order.Products.Add(product);

using (var context = new Context(configurationOptions))
{
    context.Database.EnsureDeleted();
    context.Database.EnsureCreated();

    Console.WriteLine("Order przed dodaniem do kontekstu: " + context.Entry(order).State);
    Console.WriteLine("Product przed dodaniem do kontekstu: " + context.Entry(product).State);

    //context.Attach(order);
    context.Add(order);

    Console.WriteLine("Order po dodaniu do kontekstu: " + context.Entry(order).State);
    Console.WriteLine("Product po dodaniu do kontekstu: " + context.Entry(product).State);

    context.SaveChanges();

    Console.WriteLine("Order po zapisie: " + context.Entry(order).State);
    Console.WriteLine("Product po zapisie: " + context.Entry(product).State);

    order.DateTime = DateTime.Now;

    Console.WriteLine("Order po zmianie daty: " + context.Entry(order).State);
    Console.WriteLine("Order DateTime po zmianie daty: " + context.Entry(order).Property(x => x.DateTime).IsModified);
    Console.WriteLine("Order Products po zmianie daty: " + context.Entry(order).Collection(x => x.Products).IsModified);

    context.Remove(product);
    Console.WriteLine("Order Products po usunięciu: " + context.Entry(order).Collection(x => x.Products).IsModified);
    Console.WriteLine("Product po usunięciu: " + context.Entry(product).State);

    context.SaveChanges();

    Console.WriteLine("Order po zapisie: " + context.Entry(order).State);
    Console.WriteLine("Product po zapisie: " + context.Entry(product).State);

    product.Id = 0;
    context.Attach(product);

    Console.WriteLine("Order Products po dodaniu: " + context.Entry(order).Collection(x => x.Products).IsModified);
    Console.WriteLine("Product po dodaniu: " + context.Entry(product).State);

    context.SaveChanges();

    context.ChangeTracker.Clear();


    context.Attach(order);
    Console.WriteLine("Order: " + context.Entry(order).State);
    context.Entry(order).State = EntityState.Modified;
    context.Entry(order).Property(x => x.DateTime).IsModified = true;

    context.SaveChanges();

}

using (var context = new Context(configurationOptions))
{
    //AutoDetectChangesEnabled dziala w przypadku wywołania SaveChanges, Local, Entry
    //context.ChangeTracker.AutoDetectChangesEnabled = false;

    order = new Order();
    product = new Product() { Name = "kapusta", Price = 10 };
    order.Products.Add(product);
    product = new Product() { Name = "kapusta", Price = 10 };
    order.Products.Add(product);

    Console.WriteLine("Przed dodaniem do kontekstu:");
    Console.WriteLine(context.ChangeTracker.DebugView.ShortView);

    context.Add(order);

    Console.WriteLine("Po dodaniu do kontekstu:");
    Console.WriteLine(context.ChangeTracker.DebugView.ShortView);
    Console.WriteLine(context.ChangeTracker.DebugView.LongView);

    context.SaveChanges();
    Console.WriteLine("Po zapisie:");
    Console.WriteLine(context.ChangeTracker.DebugView.ShortView);
    Console.WriteLine(context.ChangeTracker.DebugView.LongView);

    order.DateTime = DateTime.Now;
    product.Price = 2.3f;


    Console.WriteLine(context.ChangeTracker.DebugView.ShortView);
    Console.WriteLine(context.ChangeTracker.DebugView.LongView);

    context.Entry(order);
    //context.ChangeTracker.DetectChanges(); // reczne wykrywanie zmian

    Console.WriteLine(context.ChangeTracker.DebugView.ShortView);
    Console.WriteLine(context.ChangeTracker.DebugView.LongView);
}

