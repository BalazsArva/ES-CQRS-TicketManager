using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TicketManager.Common.Http;
using TicketManager.WebAPI.Controllers.Abstractions;
using TicketManager.WebAPI.DTOs.Queries;

namespace TicketManager.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketQueriesController : QueryControllerBase
    {
        private static readonly string DefaultOrderDirection = OrderDirection.Ascending.ToString();
        private static readonly string DefaultOrderByProperty = SearchTicketsQueryRequest.OrderByProperty.Id.ToString();

        private readonly IMediator mediator;

        public TicketQueriesController(IMediator mediator)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [HttpGet]
        [Route("page/{page:int}/pagesize/{pageSize:int}", Name = RouteNames.Tickets_Queries_Get_ByCriteria)]
        public async Task<IActionResult> SearchTickets([FromRoute]int page, [FromRoute]int pageSize, [FromQuery]string title, [FromQuery]string createdBy, [FromQuery]string lastUpdatedBy, [FromQuery]string orderBy, [FromQuery]string orderDirection, CancellationToken cancellationToken)
        {
            // TODO: Pass date of ceation/last update filter values if provided. Pay attention to precision, e.g. don't require to provide fractional seconds.
            var searchRequest = new SearchTicketsQueryRequest(page, pageSize, title, createdBy, lastUpdatedBy, null, null, orderBy ?? DefaultOrderByProperty, orderDirection ?? DefaultOrderDirection);
            var results = await mediator.Send(searchRequest, cancellationToken).ConfigureAwait(false);

            return FromQueryResult(results);
        }

        [HttpGet]
        [Route("{id:int}", Name = RouteNames.Tickets_Queries_Get_ById_Basic)]
        public async Task<IActionResult> GetTicketBasicDetailsById([FromRoute]long id, CancellationToken cancellationToken, [FromHeader(Name = StandardRequestHeaders.IfNoneMatch)]string eTags = null)
        {
            var eTagArray = (eTags ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries);

            var request = new GetTicketBasicDetailsByIdQueryRequest(id, eTagArray);
            var result = await mediator.Send(request, cancellationToken).ConfigureAwait(false);

            return FromQueryResult(result);
        }

        [HttpGet]
        [Route("{id:int}/extended", Name = RouteNames.Tickets_Queries_Get_ById_Extended)]
        public async Task<IActionResult> GetTicketExtendedDetailsById([FromRoute]long id, CancellationToken cancellationToken, [FromHeader(Name = StandardRequestHeaders.IfNoneMatch)]string eTags = null)
        {
            // TODO: Review route
            var eTagArray = (eTags ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries);

            var request = new GetTicketExtendedDetailsByIdQueryRequest(id, eTagArray);
            var result = await mediator.Send(request, cancellationToken).ConfigureAwait(false);

            return FromQueryResult(result);
        }

        [HttpHead]
        [Route("{id:int}", Name = RouteNames.Tickets_Queries_Head_ById)]
        public async Task<IActionResult> GetTicketMetaData([FromRoute]long id, CancellationToken cancellationToken)
        {
            var request = new TicketExistsQueryRequest(id);
            var result = await mediator.Send(request, cancellationToken).ConfigureAwait(false);

            return FromQueryResult(result);
        }

        [HttpGet]
        [Route("{id:int}/history", Name = RouteNames.Tickets_Queries_History_ById)]
        public async Task<IActionResult> GetTicketHistoryById([FromRoute]long id, CancellationToken cancellationToken, string historyTypes = null)
        {
            var request = new GetTicketHistoryQueryRequest(id, historyTypes);

            var result = await mediator.Send(request, cancellationToken).ConfigureAwait(false);
            return FromQueryResult(result);
        }
    }
}