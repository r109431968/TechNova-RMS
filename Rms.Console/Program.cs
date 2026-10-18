// Composition root only: register services, build the container, start the app.
using Microsoft.Extensions.DependencyInjection;
using Rms.Application;
using Rms.ConsoleUI;
using Rms.Infrastructure;
using Rms.Infrastructure.Seeding;

var dataDirectory = Path.Combine(AppContext.BaseDirectory, "data");

var services = new ServiceCollection();
services.AddApplication();
services.AddInfrastructure(dataDirectory);
services.AddTransient<ConsoleMenu>();

using var provider = services.BuildServiceProvider(new ServiceProviderOptions
{
    ValidateScopes = true,
    ValidateOnBuild = true
});

provider.GetRequiredService<DataSeeder>().SeedIfEmpty();
provider.GetRequiredService<ConsoleMenu>().Run();
