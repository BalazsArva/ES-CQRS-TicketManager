using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using TicketManager.Contracts.QueryApi.Models;
using TicketManager.DataAccess.Documents.Indexes;
using TicketManager.WebAPI.DTOs.Queries;
using TicketManager.WebAPI.DTOs.Queries.Abstractions;

namespace TicketManager.WebAPI.Services.QueryHandlers
{
    public class SearchTagsQueryRequestHandler : IRequestHandler<SearchTagsQueryRequest, QueryResult<TagSearchResultViewModel>>
    {
        private readonly IDocumentStore documentStore;

        public SearchTagsQueryRequestHandler(IDocumentStore documentStore)
        {
            this.documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));
        }

        public async Task<QueryResult<TagSearchResultViewModel>> Handle(SearchTagsQueryRequest request, CancellationToken cancellationToken)
        {
            using (var session = documentStore.OpenAsyncSession())
            {
                var tags = await session
                    .Query<Tickets_ByTags.IndexEntry, Tickets_ByTags>()
                    .Where(e => e.Tag.StartsWith(request.Query))
                    .OrderBy(e => e.Tag)
                    .Select(e => e.Tag)
                    .Distinct()
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);

                return new QueryResult<TagSearchResultViewModel>(new TagSearchResultViewModel { Tags = tags });
            }
        }
    }
}