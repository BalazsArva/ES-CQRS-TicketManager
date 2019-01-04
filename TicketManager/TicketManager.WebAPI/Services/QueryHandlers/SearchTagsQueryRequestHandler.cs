using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using TicketManager.Common.Utils;
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
                var query = session.Query<Tickets_ByTags.IndexEntry, Tickets_ByTags>().Statistics(out var stats);

                if (!string.IsNullOrEmpty(request.Query))
                {
                    var (kind, transformedValue) = StringSearchHelper.ParseSearchSyntax(request.Query);

                    if (kind == StringSearchKind.EndsWith)
                    {
                        query = query.Where(e => e.Tag.EndsWith(transformedValue));
                    }
                    else if (kind == StringSearchKind.StartsWith)
                    {
                        query = query.Where(e => e.Tag.StartsWith(transformedValue));
                    }
                    else if (kind == StringSearchKind.Equals)
                    {
                        query = query.Where(e => e.Tag == transformedValue);
                    }
                    else
                    {
                        query = query.Search(t => t.Tag, transformedValue);
                    }
                }

                var tags = await query
                    .OrderBy(e => e.Tag)
                    .Select(e => e.Tag)
                    .Distinct()
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);

                return new QueryResult<TagSearchResultViewModel>(
                    new TagSearchResultViewModel
                    {
                        Tags = tags,
                        IsStale = stats.IsStale,
                        IndexTimestamp = stats.IndexTimestamp
                    });
            }
        }
    }
}