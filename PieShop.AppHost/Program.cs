using System.Diagnostics;

var builder = DistributedApplication.CreateBuilder(args);

#region Redis Cache

var cache = builder.AddRedis("outputcache")
        .WithRedisCommander();

#endregion

#region SQL Server Database

Func<ExecuteCommandContext, Task<ExecuteCommandResult>> myFunc = async (context) =>
{

    try
    {
        // Ejecutar el comando usando Bash
        Process process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = "-c \"chown -R mssql:mssql /var/opt/mssql && /opt/mssql/bin/sqlservr\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();

        string output = await process.StandardOutput.ReadToEndAsync();
        string error = await process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        // Verificar el código de salida
        if (process.ExitCode != 0)
        {
            return new ExecuteCommandResult
            {
                Success = false,
                ErrorMessage = $"Command failed with error: {error}"
            };
        }

        return new ExecuteCommandResult
        {
            Success = true,
            ErrorMessage = $"Command executed successfully: {output}"
        };
    }
    catch (Exception ex)
    {
        return new ExecuteCommandResult
        {
            Success = false,
            ErrorMessage = $"Exception occurred: {ex.Message}"
        };
    }
};

var sqlPassword = builder.AddParameter("sqlPassword", secret: true);
var sqlServer = builder.AddSqlServer("sql-server", password: sqlPassword, port: 1433)
    .WithDataVolume()
////    .WithCommand("setOwnership", "SetOwnership", myFunc)
    .AddDatabase("PieShop");

#endregion

#region Database Initialization

// This should not be run in prod env
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
    ////.WithReference(databaseInitialization!)
    ////.WaitFor(databaseInitialization!)
    .WithExternalHttpEndpoints();

#endregion

await builder.Build().RunAsync();
