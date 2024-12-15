using System.Data.Common;
using System.Reflection;
using DbUp;
using DbUp.Engine;
using JackpotPlot.Prediction.API.DatabaseMigration.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace JackpotPlot.Prediction.API.DatabaseMigration
{
    public static class DatabaseMigrationInstaller
    {
        public static IServiceCollection AddLotteryApiDatabaseMigrationServices(this IServiceCollection services, string? connectionString)
        {
            Log.Logger.Information("Executing database migration at {@DatabaseMigrationStartTime}", DateTime.UtcNow);
            
            try
            {
                var upgradeEngine =
                    DeployChanges.To
                        .PostgresqlDatabase(connectionString)
                        // PreDeployment scripts
                        .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly(), script => script.StartsWith("JackpotPlot.Prediction.API.DatabaseMigration.Scripts.PreDeployment."), new SqlScriptOptions { RunGroupOrder = 1 })
                        // Migration scripts
                        .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly(), script => script.StartsWith("JackpotPlot.Prediction.API.DatabaseMigration.Scripts.Migrations."), new SqlScriptOptions { RunGroupOrder = 2 })
                        // PostDeployment scripts
                        .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly(), script => script.StartsWith("JackpotPlot.Prediction.API.DatabaseMigration.Scripts.PostDeployment."), new SqlScriptOptions { RunGroupOrder = 3 })
                        .LogToAutodetectedLog()
                        .Build();

                EnsureDatabase.For.PostgresqlDatabase(connectionString);

                var result = upgradeEngine.PerformUpgrade();

                if (result.Successful)
                {
                    Log.Logger.Information("Database migration successfully executed at {@DatabaseMigrationEndTime}!", DateTime.UtcNow);
                }
                else
                {
                    Log.Logger.Fatal(result.Error.Message);
                    throw new DatabaseMigrationException("Database migration failed. Stopping system! Please see the logs!");
                }
            }
            catch (DbException ex)
            {
                Log.Logger.Fatal(ex, "Database migration failed");
                throw new DatabaseMigrationException("Database migration failed. Stopping system! Please see the logs!", ex);
            }

            return services;
        }
    }
}
