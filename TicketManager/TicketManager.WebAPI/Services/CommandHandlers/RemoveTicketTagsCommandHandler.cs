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
    // TODO: Consider implementing a resiliency base class (e.g. mediator.Publish fails, or a constraint violation happens after validation but before insert)
    public class RemoveTicketTagsCommandHandler : IRequestHandler<RemoveTicketTagsCommand>
    {
        private readonly IMediator mediator;
        private readonly IEventsContextFactory eventsContextFactory;
        private readonly RemoveTicketTagsCommandValidator removeTicketTagsCommandValidator;

        public RemoveTicketTagsCommandHandler(IMediator mediator, IEventsContextFactory eventsContextFactory, RemoveTicketTagsCommandValidator removeTicketTagsCommandValidator)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));
            this.removeTicketTagsCommandValidator = removeTicketTagsCommandValidator ?? throw new ArgumentNullException(nameof(removeTicketTagsCommandValidator));
        }

        public async Task<Unit> Handle(RemoveTicketTagsCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await removeTicketTagsCommandValidator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            using (var context = eventsContextFactory.CreateContext())
            {
                foreach (var tag in request.Tags)
                {
                    var ticketTagChangedEvent = new TicketTagChangedEvent
                    {
                        CausedBy = request.User,
                        Tag = tag,
                        TagAdded = false,
                        TicketCreatedEventId = request.TicketId,
                        UtcDateRecorded = DateTime.UtcNow
                    };

                    // TODO: Consider whether: there should be validation that the tag is already assigned to the ticket, OR simply ignore as the query won't return it anyway.
                    context.TicketTagChangedEvents.Add(ticketTagChangedEvent);
                }

                await context.SaveChangesAsync();
            }

            await mediator.Publish(new TicketTagsRemovedNotification(request.TicketId));

            return Unit.Value;
        }
    }
}