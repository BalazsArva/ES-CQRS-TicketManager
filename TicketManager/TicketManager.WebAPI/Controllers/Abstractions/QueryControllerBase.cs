using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TicketManager.Common.Http;
using TicketManager.WebAPI.DTOs.Queries.Abstractions;

namespace TicketManager.WebAPI.Controllers.Abstractions
{
    public abstract class QueryControllerBase : ControllerBase
    {
        protected IActionResult FromQueryResult<TResponse>(QueryResult<TResponse> queryResult)
        {
            if (queryResult == null)
            {
                return NotFound();
            }

            if (queryResult.ResultType == QueryResultType.NotModified)
            {
                return StatusCode(StatusCodes.Status304NotModified);
            }

            if (!string.IsNullOrWhiteSpace(queryResult.ETag))
            {
                Response.Headers[StandardResponseHeaders.ETag] = queryResult.ETag;
            }

            return Ok(queryResult.Result);
        }

        protected IActionResult FromQueryResult(GetTicketMetadataQueryResult existenceCheckQueryResult)
        {
            if (existenceCheckQueryResult == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrWhiteSpace(existenceCheckQueryResult.ETag))
            {
                Response.Headers[StandardResponseHeaders.ETag] = existenceCheckQueryResult.ETag;
            }

            return Ok();
        }
    }
}