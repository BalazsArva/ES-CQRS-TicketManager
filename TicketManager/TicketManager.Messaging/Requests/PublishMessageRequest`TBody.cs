﻿using System.Collections.Generic;
using TicketManager.Common.Utils;

namespace TicketManager.Messaging.Requests
{
    public class PublishMessageRequest<TBody> where TBody : class
    {
        public PublishMessageRequest(TBody body, string correlationId, IReadOnlyDictionary<string, string> headers = null)
        {
            Throw.IfNullOrWhiteSpace(nameof(correlationId), correlationId);
            Throw.IfNull(nameof(body), body);

            Body = body;
            CorrelationId = correlationId;
            Headers = headers ?? new Dictionary<string, string>();
        }

        public TBody Body { get; }

        public string CorrelationId { get; }

        public IReadOnlyDictionary<string, string> Headers { get; }
    }
}