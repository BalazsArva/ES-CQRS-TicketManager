using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raven.Client.Documents;
using Raven.Client.Documents.Operations;
using Raven.Client.Exceptions;
using Raven.Client.Exceptions.Database;
using Raven.Client.ServerWide;
using Raven.Client.ServerWide.Operations;

namespace TicketManager.DataAccess.Documents.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRavenDb(this IServiceCollection services, IConfiguration configuration)
        {
            var ravenDbUrls = configuration.GetSection("DataAccess:RavenDb:Urls").Get<string[]>();
            var ravenDbDatabase = configuration["DataAccess:RavenDb:Database"];

            EnsureDatabaseExists(ravenDbUrls, ravenDbDatabase);

            var store = new DocumentStore
            {
                Urls = ravenDbUrls,
                Database = ravenDbDatabase
            }.Initialize();

            IndexCreator.CreateIndexes(store);
            services.AddSingleton(store);

            return services;
        }

        private static void EnsureDatabaseExists(string[] urls, string database)
        {
            if (string.IsNullOrWhiteSpace(database))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(database));
            }

            var store = new DocumentStore { Urls = urls }.Initialize();

            try
            {
                store.Maintenance.ForDatabase(database).Send(new GetStatisticsOperation());
            }
            catch (DatabaseDoesNotExistException)
            {
                try
                {
                    store.Maintenance.Server.Send(new CreateDatabaseOperation(new DatabaseRecord(database), 3));
                }
                catch (ConcurrencyException)
                {
                    // The database was already created before calling CreateDatabaseOperation
                }
            }
        }
    }
}