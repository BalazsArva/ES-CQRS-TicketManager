using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TicketManager.WebAPI.Controllers.Abstractions;
using TicketManager.WebAPI.DTOs.Queries;

namespace TicketManager.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TagQueriesController : QueryControllerBase
    {
        private readonly IMediator mediator;

        public TagQueriesController(IMediator mediator)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery]string query)
        {
            var result = await mediator.Send(new SearchTagsQueryRequest(query ?? string.Empty));

            return FromQueryResult(result);
        }
    }
}