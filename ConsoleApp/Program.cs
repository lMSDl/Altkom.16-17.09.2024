using DAL;
using Microsoft.EntityFrameworkCore;

var configurationOptions = new DbContextOptionsBuilder<Context>()
    .UseSqlServer(@"Server=(local);Database=EfCore;Integrated security=true;TrustServerCertificate=true")
    .Options;

using (var context = new Context(configurationOptions))
{
    context.Database.EnsureCreated();

}