using DbMigrator.Interfaces;
using DbMigrator.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Data.Common;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();
builder.Services.AddScoped<IDbMigrator, MigrationService>();

var host = builder.Build();

var sourceConn = "Server=(localdb)\\MSSQLLocalDB;Database=MyCompanyDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;";
var targetConn = "Server=(localdb)\\MSSQLLocalDB;Database=TargetDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;";

var dbMigrator = host.Services.GetRequiredService<IDbMigrator>();
await dbMigrator.MigrateAsync(sourceConn, targetConn);
host.Run();