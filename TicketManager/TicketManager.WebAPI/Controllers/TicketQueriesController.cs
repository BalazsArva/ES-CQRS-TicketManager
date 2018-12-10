using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TicketManager.WebAPI.Controllers.Abstractions;
using TicketManager.WebAPI.DTOs.Queries;

namespace TicketManager.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketQueriesController : QueryControllerBase
    {
        private readonly IMediator mediator;

        public TicketQueriesController(IMediator mediator)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [HttpGet]
        [Route("page/{page:int}/pagesize/{pageSize:int}", Name = RouteNames.Tickets_Queries_Get_ByCriteria)]
        public async Task<IActionResult> SearchTickets([FromRoute]int page, [FromRoute]int pageSize, [FromQuery]string title, [FromQuery]string createdBy, CancellationToken cancellationToken)
        {
            var searchRequest = new SearchTicketsQueryRequest(page, pageSize, title, createdBy);
            var results = await mediator.Send(searchRequest, cancellationToken);

            return FromQueryResult(results);
        }

        [HttpGet]
        [Route("{id:int}", Name = RouteNames.Tickets_Queries_Get_ById)]
        public Task<IActionResult> GetTicketById([FromRoute]long id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        [HttpHead]
        [Route("{id:int}", Name = RouteNames.Tickets_Queries_Head_ById)]
        public async Task<IActionResult> GetTicketMetaData([FromRoute]long id, CancellationToken cancellationToken)
        {
            var request = new TicketExistsRequest(id);
            var result = await mediator.Send(request, cancellationToken);

            return FromQueryResult(result);
        }
    }
}