using Core.Common;
using Core.Common.Migrations;
using Tutor;
using Tutor.Infrastructure;
using Tutor.Infrastructure.Migrations;

var host = Host
    .CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
    {
        services.AddCore(Host.CreateDefaultBuilder(args));
    })
    .ConfigureWebHostDefaults(builder =>
        builder.UseStartup<Startup>())
    .Build();

var migratedHost = host.RunMigrations<MigrationMarker>();

using (var scope = migratedHost.Services.CreateScope())
{
    await DbInitializer.SeedAsync(scope.ServiceProvider); 
}

migratedHost.Run();