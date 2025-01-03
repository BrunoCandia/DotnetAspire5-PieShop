var builder = DistributedApplication.CreateBuilder(args);

#region Redis Cache

var cache = builder.AddRedis("outputcache")
        .WithRedisCommander();

#endregion

#region SQL Server Database

var sqlPassword = builder.AddParameter("sqlPassword", secret: true);
var sqlServer = builder.AddSqlServer("sqlServer", password: sqlPassword)
    .WithDataVolume()
    .AddDatabase("PieShop");

#endregion

#region Database Initialization

IResourceBuilder<ProjectResource>? databaseInitialization = null;

databaseInitialization = builder.AddProject<Projects.PieShop_Database_Initialization>("pieshop-database-initialization")
    .WithReference(sqlServer)
    .WaitFor(sqlServer);

#endregion

#region Web Site

builder.AddProject<Projects.PieShop_UI>("pieshop-ui")
    .WithReference(cache)
    .WithReference(sqlServer)
    .WaitFor(sqlServer)
    .WithReference(databaseInitialization!)
    .WaitFor(databaseInitialization!);

#endregion

await builder.Build().RunAsync();
