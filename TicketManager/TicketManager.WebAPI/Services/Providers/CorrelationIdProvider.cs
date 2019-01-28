using System;
using Microsoft.AspNetCore.Http;

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
            return httpContextAccessor.HttpContext.TraceIdentifier;
        }
    }
}