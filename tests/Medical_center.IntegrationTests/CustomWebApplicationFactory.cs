using Medical_center.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using System.Linq;

namespace Medical_center.IntegrationTests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override IHost CreateHost(IHostBuilder builder)
        {
            var builtHost = base.CreateHost(builder);

            using (var scope = builtHost.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                var serviceCollection = new ServiceCollection();

                foreach (var descriptor in builder.GetType()
                    .GetField("_services", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.GetValue(builder) as IServiceCollection ?? new ServiceCollection())
                {
                    if (descriptor.ServiceType != typeof(DbContextOptions<ApplicationDbContext>))
                    {
                        serviceCollection.Add(descriptor);
                    }
                }
                serviceCollection.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDatabase");
                });

                var sp = serviceCollection.BuildServiceProvider();

                using (var dbScope = sp.CreateScope())
                {
                    var db = dbScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    db.Database.EnsureDeleted();
                    db.Database.EnsureCreated();
                }
            }

            return builtHost;
        }
    }
}
