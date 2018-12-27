using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using TicketManager.DataAccess.Events;
using TicketManager.DataAccess.Events.DataModel;
using TicketManager.WebAPI.DTOs.Commands;
using TicketManager.WebAPI.DTOs.Notifications;

namespace TicketManager.WebAPI.Services.CommandHandlers
{
    public class EditTicketCommentCommandHandler : IRequestHandler<EditTicketCommentCommand>
    {
        private readonly IMediator mediator;
        private readonly IEventsContextFactory eventsContextFactory;
        private readonly IValidator<EditTicketCommentCommand> validator;

        public EditTicketCommentCommandHandler(IMediator mediator, IEventsContextFactory eventsContextFactory, IValidator<EditTicketCommentCommand> validator)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));
            this.validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public async Task<Unit> Handle(EditTicketCommentCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            using (var context = eventsContextFactory.CreateContext())
            {
                context.TicketCommentEditedEvents.Add(new TicketCommentEditedEvent
                {
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