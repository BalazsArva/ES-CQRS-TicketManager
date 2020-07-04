using Microsoft.Extensions.Configuration;
using Raven.Client.Documents;
using Raven.Client.Documents.Operations;
using Raven.Client.Exceptions;
using Raven.Client.Exceptions.Database;
using Raven.Client.ServerWide;
using Raven.Client.ServerWide.Operations;
using System;
using System.Threading;
using System.Threading.Tasks;
using TicketManager.DataAccess.Documents;
using TicketManager.WebAPI.StartupTasks.Abstractions;

namespace TicketManager.WebAPI.StartupTasks
{
    public class SetupDocumentsDatabase : IApplicationStartupTask
    {
        private readonly IConfiguration configuration;
        private readonly IDocumentStore documentStore;

        public SetupDocumentsDatabase(IConfiguration configuration, IDocumentStore documentStore)
        {
            this.configuration = configuration;
            this.documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));
        }

        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            EnsureDatabaseExists();
            CreateIndexes();

            return Task.CompletedTask;
        }

        private void EnsureDatabaseExists()
        {
            var database = configuration["DataAccess:RavenDb:Database"];

            try
            {
                documentStore.Maintenance.ForDatabase(database).Send(new GetStatisticsOperation());
            }
            catch (DatabaseDoesNotExistException)
            {
                try
                {
                    documentStore.Maintenance.Server.Send(new CreateDatabaseOperation(new DatabaseRecord(database), 1));
                }
                catch (ConcurrencyException)
                {
                    // The database was already created before calling CreateDatabaseOperation
                }
            }
        }

        private void CreateIndexes()
        {
            IndexCreator.CreateIndexes(documentStore);
        }
    }
}