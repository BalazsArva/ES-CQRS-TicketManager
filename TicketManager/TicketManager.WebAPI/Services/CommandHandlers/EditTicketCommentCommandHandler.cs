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
    public class EditTicketCommentCommandHandler : IRequestHandler<EditTicketCommentCommand>
    {
        private readonly IMediator mediator;
        private readonly IEventsContextFactory eventsContextFactory;
        private readonly IValidator<EditTicketCommentCommand> validator;
        private readonly ICorrelationIdProvider correlationIdProvider;

        public EditTicketCommentCommandHandler(ICorrelationIdProvider correlationIdProvider, IMediator mediator, IEventsContextFactory eventsContextFactory, IValidator<EditTicketCommentCommand> validator)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));
            this.validator = validator ?? throw new ArgumentNullException(nameof(validator));
            this.correlationIdProvider = correlationIdProvider ?? throw new ArgumentNullException(nameof(correlationIdProvider));
        }

        public async Task<Unit> Handle(EditTicketCommentCommand request, CancellationToken cancellationToken)
        {
            await validator.ValidateAndThrowAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);

            using (var context = eventsContextFactory.CreateContext())
            {
                context.TicketCommentEditedEvents.Add(new TicketCommentEditedEvent
                {
                    CorrelationId = correlationIdProvider.GetCorrelationId(),
                    CausedBy = request.RaisedByUser,
                    CommentText = request.CommentText,
                    TicketCommentPostedEventId = request.CommentId
                });

                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            await mediator.Publish(new TicketCommentEditedNotification(request.CommentId), cancellationToken).ConfigureAwait(false);

            return Unit.Value;
        }
    }
}