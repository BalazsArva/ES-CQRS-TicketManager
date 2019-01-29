using System;
using Microsoft.AspNetCore.Http;
using TicketManager.Common.Http;

namespace TicketManager.WebAPI.Services.Providers
{
    public class CorrelationIdProvider : ICorrelationIdProvider
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public CorrelationIdProvider(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public string GetCorrelationId()
        {
            var headers = httpContextAccessor.HttpContext.Request.Headers;
            if (headers.ContainsKey(CustomRequestHeaders.CorrelationId) && !string.IsNullOrEmpty(headers[CustomRequestHeaders.CorrelationId]))
            {
                return headers[CustomRequestHeaders.CorrelationId];
            }

            return httpContextAccessor.HttpContext.TraceIdentifier;
        }
    }
}