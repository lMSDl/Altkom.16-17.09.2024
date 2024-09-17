using DAL.Conventions;
using DAL.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Models;

namespace DAL
{
    public class Context : DbContext
    {
        public static Func<Context, DateTime, DateTime, IEnumerable<Order>> GetOrdersByDateRange { get; } =
            EF.CompileQuery((Context context, DateTime from, DateTime to) =>
            context.Set<Order>().Include(x => x.Products).Where(x => x.DateTime > from)
            .Where(x => x.DateTime < to)
            );

        public Context(DbContextOptions options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //modelBuilder.UsePropertyAccessMode(PropertyAccessMode.PreferProperty);

            modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);


            //modelBuilder.HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangedNotifications);


            modelBuilder.Model.GetEntityTypes()
                .SelectMany(x => x.GetProperties())
                .Where(x => x.ClrType == typeof(int))
                .Where(x => x.Name == "Key")
                .ToList()
                .ForEach(x =>
                {
                    x.IsNullable = false;
                    ((IMutableEntityType)x.DeclaringType).SetPrimaryKey(x);
                });

            modelBuilder.Model.GetEntityTypes()
                .SelectMany(x => x.GetProperties())
                .Where(x => x.ClrType == typeof(string))
                .Where(x => x.PropertyInfo?.CanWrite ?? false)
                .ToList()
                .ForEach(x => x.SetValueConverter(new ObfuscationConverter()));
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            base.ConfigureConventions(configurationBuilder);

            //configurationBuilder.Properties<DateTime>().HavePrecision(5);
            configurationBuilder.Conventions.Add(_ => new DateTimeConvention());
            configurationBuilder.Conventions.Add(_ => new PluralizeTableNameConvention());

            //configurationBuilder.Conventions.Remove(typeof(KeyDiscoveryConvention));
        }

        public bool RandomFail { get; set; }
        public override int SaveChanges()
        {
            if (RandomFail && new Random((int)DateTime.Now.Ticks).Next(1, 25) == 1)
                throw new Exception();
            return base.SaveChanges();
        }
    }
}
