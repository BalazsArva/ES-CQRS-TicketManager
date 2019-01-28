using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using TicketManager.DataAccess.Events;
using TicketManager.DataAccess.Events.DataModel;
using TicketManager.WebAPI.DTOs.Commands;
using TicketManager.WebAPI.DTOs.Notifications;
using TicketManager.WebAPI.Services.Providers;

namespace TicketManager.WebAPI.Services.CommandHandlers
{
    public class PostCommentToTicketCommandHandler : IRequestHandler<PostCommentToTicketCommand, long>
    {
        private readonly IMediator mediator;
        private readonly IEventsContextFactory eventsContextFactory;
        private readonly IValidator<PostCommentToTicketCommand> validator;
        private readonly ICorrelationIdProvider correlationIdProvider;

        public PostCommentToTicketCommandHandler(ICorrelationIdProvider correlationIdProvider, IMediator mediator, IEventsContextFactory eventsContextFactory, IValidator<PostCommentToTicketCommand> validator)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));
            this.validator = validator ?? throw new ArgumentNullException(nameof(validator));
            this.correlationIdProvider = correlationIdProvider ?? throw new ArgumentNullException(nameof(correlationIdProvider));
        }

        public async Task<long> Handle(PostCommentToTicketCommand request, CancellationToken cancellationToken)
        {
            await validator.ValidateAndThrowAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);

            long commentId;
            var correlationId = correlationIdProvider.GetCorrelationId();

            using (var context = eventsContextFactory.CreateContext())
            {
                var ticketCommentPostedEvent = new TicketCommentPostedEvent
                {
                    CorrelationId = correlationId,
                    CausedBy = request.RaisedByUser,
                    TicketCreatedEventId = request.TicketId
                };

                context.TicketCommentPostedEvents.Add(ticketCommentPostedEvent);
                context.TicketCommentEditedEvents.Add(new TicketCommentEditedEvent
                {
                    CorrelationId = correlationId,
                    CausedBy = request.RaisedByUser,
                    CommentText = request.CommentText,
                    TicketCommentPostedEvent = ticketCommentPostedEvent
                });

                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                commentId = ticketCommentPostedEvent.Id;
            }

            await mediator.Publish(new TicketCommentPostedNotification(commentId), cancellationToken).ConfigureAwait(false);

            return commentId;
        }
    }
}