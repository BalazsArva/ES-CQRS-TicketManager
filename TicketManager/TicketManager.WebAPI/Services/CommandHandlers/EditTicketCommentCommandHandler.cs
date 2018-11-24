using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using TicketManager.DataAccess.Events;
using TicketManager.DataAccess.Events.DataModel;
using TicketManager.WebAPI.DTOs.Commands;
using TicketManager.WebAPI.DTOs.Notifications;
using TicketManager.WebAPI.Validation.CommandValidators;

namespace TicketManager.WebAPI.Services.CommandHandlers
{
    public class EditTicketCommentCommandHandler : IRequestHandler<EditTicketCommentCommand>
    {
        private readonly IMediator mediator;
        private readonly IEventsContextFactory eventsContextFactory;
        private readonly EditTicketCommentCommandValidator editTicketCommentCommandValidator;

        public EditTicketCommentCommandHandler(IMediator mediator, IEventsContextFactory eventsContextFactory, EditTicketCommentCommandValidator editTicketCommentCommandValidator)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));
            this.editTicketCommentCommandValidator = editTicketCommentCommandValidator ?? throw new ArgumentNullException(nameof(editTicketCommentCommandValidator));
        }

        public async Task<Unit> Handle(EditTicketCommentCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await editTicketCommentCommandValidator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            using (var context = eventsContextFactory.CreateContext())
            {
                context.TicketCommentEditedEvents.Add(new TicketCommentEditedEvent
                {
                    CausedBy = request.User,
                    CommentText = request.CommentText,
                    TicketCommentPostedEventId = request.CommentId,
                    UtcDateRecorded = DateTime.UtcNow
                });

                await context.SaveChangesAsync();
            }

            await mediator.Publish(new TicketCommentEditedNotification(request.CommentId));

            return Unit.Value;
        }
    }
}