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

// 2. Создаете Scope и передаете провайдер сервисов внутрь метода
using (var scope = migratedHost.Services.CreateScope())
{
    // Вот здесь вызывается ваш статический класс
    await DbInitializer.SeedAsync(scope.ServiceProvider); 
}

migratedHost.Run();