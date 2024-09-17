using DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.IdentityModel.Protocols;
using NetTopologySuite.Geometries;
using Models;
using System.Diagnostics;

var configurationOptions = new DbContextOptionsBuilder<Context>()
    .UseSqlServer(@"Server=(local);Database=EfCore;Integrated security=true;TrustServerCertificate=true",
    x => x.UseNetTopologySuite())
    .LogTo(Console.WriteLine)
    //.UseChangeTrackingProxies()
    //.UseLazyLoadingProxies()
    .Options;


using (var context = new Context(configurationOptions))
{
    context.Database.EnsureDeleted();
    context.Database.Migrate();
}

using (var context = new Context(configurationOptions))
{

    var person = new Person() { Name = "Ewa", Address = new Address() { City = "Katowice", Street = "Krakowska", Number = 19, Coordinates = new Models.Coordinates { Longitude = 50, Latitude = 19 } } };
    context.Add(person);

    person = new Person() { Name = "Adam", Address = new Address() { City = "Kraków", Street = "Katowicka", Number = 91, Coordinates = new Models.Coordinates { Longitude = 51, Latitude = 20 } } };
    context.Add(person);

    context.SaveChanges();

    context.ChangeTracker.Clear();

    person = context.Set<Person>().Where(x => x.Address.Coordinates.Latitude == 20).FirstOrDefault();
    person.Address.Number = 22;
    context.SaveChanges();

}



    using (var context = new Context(configurationOptions))
{
    var view = context.Set<OrderSummary>().Where(x => x.Id > 2).ToList();
}

static void ChangeTracker(DbContextOptions<Context> configurationOptions)
{
    var order = new Order();
    var product = new Product() { Name = "marchewka", Price = 15 };

    order.Products.Add(product);

    using (var context = new Context(configurationOptions))
    {

        context.ChangeTracker.AutoDetectChangesEnabled = false;

        Console.WriteLine("Order przed dodaniem do kontekstu: " + context.Entry(order).State);
        Console.WriteLine("Product przed dodaniem do kontekstu: " + context.Entry(product).State);

        //order = context.CreateProxy<Order>();
        //product = context.CreateProxy<Product>(x => { x.Price = product.Price; x.Name = product.Name; });
        order.Products.Add(product);

        //context.Attach(order);
        context.Add(order);

        Console.WriteLine("Order po dodaniu do kontekstu: " + context.Entry(order).State);
        Console.WriteLine("Product po dodaniu do kontekstu: " + context.Entry(product).State);

        context.SaveChanges();

        Console.WriteLine("Order po zapisie: " + context.Entry(order).State);
        Console.WriteLine("Product po zapisie: " + context.Entry(product).State);

        order.DateTime = DateTime.Now;


        Console.WriteLine(context.ChangeTracker.DebugView.LongView);

        Console.WriteLine("Order po zmianie daty: " + context.Entry(order).State);
        Console.WriteLine("Order DateTime po zmianie daty: " + context.Entry(order).Property(x => x.DateTime).IsModified);
        Console.WriteLine("Order Products po zmianie daty: " + context.Entry(order).Collection(x => x.Products).IsModified);

        context.Remove(product);
        Console.WriteLine("Order Products po usunięciu: " + context.Entry(order).Collection(x => x.Products).IsModified);
        Console.WriteLine("Product po usunięciu: " + context.Entry(product).State);

        context.SaveChanges();

        Console.WriteLine("Order po zapisie: " + context.Entry(order).State);
        Console.WriteLine("Product po zapisie: " + context.Entry(product).State);

        //context.ChangeTracker.Clear();
        product.Id = 0;
        //product.Order = new Order() { Id = 1};
        context.Add(product);
        //context.Entry(product.Order).State = EntityState.Unchanged;

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


    using (var context = new Context(configurationOptions))
    {
        //AsNoTracking pomija context przy pobieraniu danych
        context.Set<Product>().AsNoTracking().ToList();
        Console.WriteLine("----");
        Console.WriteLine(context.ChangeTracker.DebugView.LongView);


    }
}

static void ConcurrencyCheck(DbContextOptions<Context> configurationOptions)
{
    using (var context = new Context(configurationOptions))
    {
        var order = new Order();
        order.DateTime = DateTime.Now;

        context.Add(order);
        context.SaveChanges();

        order.DateTime = DateTime.Now.AddDays(-100);
        context.SaveChanges();

        var product = new Product() { Order = order, Name = "marchewka", Price = 15 };
        context.Add(product);
        context.SaveChanges();

        product.Price = product.Price * 1.1f;
        var saved = false;
        while (!saved)
        {
            try
            {
                context.SaveChanges();
                saved = true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                foreach (var entry in ex.Entries)
                {
                    //wartości jakie chcemy wprowadzić do bazy
                    var currentValues = entry.CurrentValues;
                    //wartości jakie pobraliśmy z bazy
                    var originalValues = entry.OriginalValues;
                    //wartości aktualne w bazie
                    var databaseValues = entry.GetDatabaseValues();

                    switch (entry.Entity)
                    {
                        case Product:

                            var property = currentValues.Properties.Single(x => x.Name == nameof(Product.Price));
                            var currentValue = (float)currentValues[property];
                            var originalValue = (float)originalValues[property];
                            var databaseValue = (float)databaseValues[property];

                            currentValue = databaseValue + (currentValue - originalValue);

                            currentValues[property] = currentValue;

                            break;
                    }
                    entry.OriginalValues.SetValues(databaseValues);
                }

            }
        }
    }
}

static void SHadowProperty_QueryFiters(DbContextOptions<Context> configurationOptions)
{
    using (var context = new Context(configurationOptions))
    {
        for (int i = 0; i < 17; i++)
        {
            var order = new Order();
            order.DateTime = DateTime.Now;
            var orderProduct = new Product { Name = "P" + i, Price = 1 + i };
            order.Products.Add(orderProduct);

            context.Add(order);
        }

        context.SaveChanges();

        context.ChangeTracker.Clear();

        var product = context.Set<Product>().Skip(5).First();
        var orderId = context.Entry(product).Property<int>("OrderId").CurrentValue;
        orderId = context.Set<Product>().Skip(4).Select(x => EF.Property<int>(x, "OrderId")).First();

        context.Entry(product).Property("OrderId").CurrentValue = 5;
        context.SaveChanges();

        var products = context.Set<Product>().Where(x => EF.Property<int>(x, "OrderId") == 5).ToList();

        //product.IsDeleted = true;
        context.Entry(product).Property<bool>("IsDeleted").CurrentValue = true;
        context.SaveChanges();

        context.ChangeTracker.Clear();


        products = context.Set<Product>().ToList();

        var orders = context.Set<Order>().Include(x => x.Products).ToList();

        orders = context.Set<Order>().IgnoreQueryFilters().Include(x => x.Products).ToList();

    }
}

static void Transactions(DbContextOptions<Context> configurationOptions, bool randomFail = true)
{
    using (var context = new Context(configurationOptions))
    {
        context.RandomFail = randomFail;
        var products = Enumerable.Range(100, 50).Select(x => new Product { Name = $"Product {x}", Price = 1.23f * x, Details = new ProductDetails { Height = x, Width = 2 * x, Weight = 3 * x } }).ToList();
        var orders = Enumerable.Range(0, 5).Select(x => new Order
        {
            Name = x.ToString(),
            DateTime = DateTime.Now.AddMinutes(-1.23f * x),
            OrderType = (OrderType)(x % 3),
            Parameters = (Parameters)(x % 16),
            DeliveryPoint = new Point(50 + 0.1 * x, 19 + 0.1 * x) { SRID = 4326 }
        }).ToList();

        using (var transaction = context.Database.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted))
        {
            for (int i = 0; i < orders.Count; i++)
            {
                string savepoint = i.ToString();
                try
                {
                    transaction.CreateSavepoint(savepoint);

                    var subproducts = products.Skip(i * 10).Take(10).ToList();

                    foreach (var prodcut in subproducts)
                    {
                        context.Add(prodcut);

                        context.SaveChanges();
                    }

                    var order = orders[i];
                    order.Products = subproducts;
                    context.Add(order);

                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(context.ChangeTracker.DebugView.ShortView);
                    transaction.RollbackToSavepoint(savepoint);
                    context.ChangeTracker.Clear();
                    Console.WriteLine(context.ChangeTracker.DebugView.ShortView);
                }
            }

            transaction.Commit();
        }

    }
}

static void LoadingData(DbContextOptions<Context> configurationOptions)
{
    Transactions(configurationOptions, false);


    using (var context = new Context(configurationOptions))
    {
        //EagerLoading
        var product = context.Set<Product>()/*.AsSplitQuery()*/.Include(x => x.Order).ThenInclude(x => x.Products).First();
    }

    using (var context = new Context(configurationOptions))
    {
        var product = context.Set<Product>().First();
        //ExpicitLoading
        context.Entry(product).Reference(x => x.Order).Load();
        context.Entry(product.Order).Collection(x => x.Products).Load();
    }

    using (var context = new Context(configurationOptions))
    {
        var products = context.Set<Product>().Take(2).ToList();

        context.Set<Order>().Where(x => products.Select(x => context.Entry(x).Property<int?>("OrderId").CurrentValue).Contains(x.Id)).Load();
    }


    using (var context = new Context(configurationOptions))
    {
        var product = context.Set<Product>().First();
        //Lazy loading
        if (product.Order != null)
            Console.WriteLine();
    }
}

static void TemporalTable(DbContextOptions<Context> configurationOptions)
{
    var person = new Person { Name = "Ewa" };

    using (var context = new Context(configurationOptions))
    {
        context.Add(person);
        context.SaveChanges();

        Thread.Sleep(1000);

        person.Name = "Ala";
        context.SaveChanges();

        Thread.Sleep(1000);

        person.Name = "Adam";
        context.SaveChanges();

        Thread.Sleep(1000);

        person.Name = "Wojciech";
        context.SaveChanges();

        context.ChangeTracker.Clear();

        person = context.Set<Person>().First();
        var people = context.Set<Person>().ToList();

        people = context.Set<Person>().TemporalAll().ToList();

        var data = context.Set<Person>().TemporalAll()
            .Select(x => new { x, FROM = EF.Property<DateTime>(x, "From"), TO = EF.Property<DateTime>(x, "To") }).ToList();

        people = context.Set<Person>().TemporalAsOf(DateTime.UtcNow.AddSeconds(-2)).ToList();

        people = context.Set<Person>().TemporalBetween(DateTime.UtcNow.AddSeconds(-4), DateTime.UtcNow.AddSeconds(-2)).ToList();
    }
}

static void CompileQuery(DbContextOptions<Context> configurationOptions)
{
    Transactions(configurationOptions, false);
    using (var context = new Context(configurationOptions))
    {
        var timer = new Stopwatch();
        timer.Reset();
        timer.Start();
        var orders = context.Set<Order>().Include(x => x.Products).ThenInclude(x => x.Details).Where(x => x.DateTime > DateTime.Now.AddMinutes(-5))
            .Where(x => x.DateTime < DateTime.Now.AddMinutes(-1)).ToList();
        timer.Stop();

        Console.WriteLine(timer.ElapsedTicks);
    }
    Console.WriteLine("--------------------");

    using (var context = new Context(configurationOptions))
    {
        var timer = new Stopwatch();
        timer.Start();
        var orders = context.Set<Order>().Include(x => x.Products).Where(x => x.DateTime > DateTime.Now.AddMinutes(-5))
            .Where(x => x.DateTime < DateTime.Now.AddMinutes(0)).ToList();
        timer.Stop();

        Console.WriteLine(timer.ElapsedTicks);
    }
    Console.WriteLine("--------------------");

    using (var context = new Context(configurationOptions))
    {
        var timer = new Stopwatch();
        timer.Reset();
        timer.Start();
        var orders = Context.GetOrdersByDateRange(context, DateTime.Now.AddMinutes(-5), DateTime.Now.AddMinutes(-1)).ToList();
        timer.Stop();

        Console.WriteLine(timer.ElapsedTicks);
    }

    Console.WriteLine("--------------------");

    using (var context = new Context(configurationOptions))
    {
        var timer = new Stopwatch();
        timer.Reset();
        timer.Start();
        var orders = Context.GetOrdersByDateRange(context, DateTime.Now.AddMinutes(-5), DateTime.Now.AddMinutes(0)).ToList();
        timer.Stop();

        Console.WriteLine(timer.ElapsedTicks);
    }
}

static void StoredProcedures(DbContextOptions<Context> configurationOptions)
{
    Transactions(configurationOptions, false);

    using (var context = new Context(configurationOptions))
    {

        var multilpier = "-1.1";
        context.Database.ExecuteSqlRaw("EXEC ChangePrice @p0", multilpier);
        context.Database.ExecuteSqlInterpolated($"EXEC ChangePrice {multilpier}");

        var result = context.Set<OrderSummary>().FromSqlInterpolated($"EXEC OrderSummary {1}").ToList();
    }
}

static void NetTopologySuite(DbContextOptions<Context> configurationOptions)
{
    Transactions(configurationOptions, false);

    using (var context = new Context(configurationOptions))
    {
        var order = context.Set<Order>().Skip(2).First();

        var point = new Point(50, 19) { SRID = 4326 };

        var distance = point.Distance(order.DeliveryPoint);

        var intersect = point.Intersects(order.DeliveryPoint);

        var polygon = new Polygon(new LinearRing(new Coordinate[] { new Coordinate(50, 19),
                                                                new Coordinate(49, 20),
                                                                new Coordinate(50, 21),
                                                                new Coordinate(51, 20),
                                                                new Coordinate(50, 19)}))
        { SRID = 4326 };

        intersect = polygon.Intersects(order.DeliveryPoint);

    }
}