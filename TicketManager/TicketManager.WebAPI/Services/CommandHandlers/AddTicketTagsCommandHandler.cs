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
    public class AddTicketTagsCommandHandler : IRequestHandler<AddTicketTagsCommand>
    {
        private readonly IMediator mediator;
        private readonly IEventsContextFactory eventsContextFactory;
        private readonly AddTicketTagsCommandValidator addTicketTagsCommandValidator;

        public AddTicketTagsCommandHandler(IMediator mediator, IEventsContextFactory eventsContextFactory, AddTicketTagsCommandValidator addTicketTagsCommandValidator)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));
            this.addTicketTagsCommandValidator = addTicketTagsCommandValidator ?? throw new ArgumentNullException(nameof(addTicketTagsCommandValidator));
        }

        public async Task<Unit> Handle(AddTicketTagsCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await addTicketTagsCommandValidator.ValidateAsync(request, cancellationToken);
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
                        TagAdded = true,
                        TicketCreatedEventId = request.TicketId,
                        UtcDateRecorded = DateTime.UtcNow
                    };

                    // TODO: Consider whether: there should be validation that the tag is not yet assigned to the ticket, OR simply ignore and get the distinct tags on the query level.
                    context.TicketTagChangedEvents.Add(ticketTagChangedEvent);
                }

                await context.SaveChangesAsync();
            }

            await mediator.Publish(new TicketTagsAddedNotification(request.TicketId));

            return Unit.Value;
        }
    }
}