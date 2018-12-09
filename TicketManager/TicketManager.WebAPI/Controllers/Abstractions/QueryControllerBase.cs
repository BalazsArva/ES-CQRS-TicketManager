using System.Net;
using Microsoft.AspNetCore.Mvc;
using TicketManager.Common.Http;
using TicketManager.WebAPI.DTOs.Queries.Abstractions;

namespace TicketManager.WebAPI.Controllers.Abstractions
{
    public abstract class QueryControllerBase : ControllerBase
    {
        public IActionResult FromQueryResult<TResponse>(QueryResult<TResponse> queryResult)
        {
            if (queryResult.ResultType == QueryResultType.NotFound)
            {
                return NotFound();
            }
            else if (queryResult.ResultType == QueryResultType.NotModified)
            {
                return StatusCode((int)HttpStatusCode.NotModified);
            }

            if (!string.IsNullOrWhiteSpace(queryResult.ETag))
            {
                Response.Headers[StandardResponseHeaders.ETag] = queryResult.ETag;
            }

            return Ok(queryResult.Result);
        }

        public IActionResult FromQueryResult(ExistenceCheckQueryResult existenceCheckQueryResult)
        {
            if (existenceCheckQueryResult == ExistenceCheckQueryResult.NotFound)
            {
                return NotFound();
            }

            return Ok();
        }
    }
}