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
    // TODO: Consider implementing a resiliency base class (e.g. mediator.Publish fails, or a constraint violation happens after validation but before insert)
    public class RemoveTicketTagsCommandHandler : IRequestHandler<RemoveTicketTagsCommand>
    {
        private readonly IMediator mediator;
        private readonly IEventsContextFactory eventsContextFactory;
        private readonly IValidator<RemoveTicketTagsCommand> validator;

        public RemoveTicketTagsCommandHandler(IMediator mediator, IEventsContextFactory eventsContextFactory, IValidator<RemoveTicketTagsCommand> validator)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));
            this.validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public async Task<Unit> Handle(RemoveTicketTagsCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            using (var context = eventsContextFactory.CreateContext())
            {
                foreach (var tag in request.Tags)
                {
                    context.TicketTagChangedEvents.Add(new TicketTagChangedEvent
                    {
                        CausedBy = request.RaisedByUser,
                        Tag = tag,
                        TagAdded = false,
                        TicketCreatedEventId = request.TicketId
                    });
                }

                await context.SaveChangesAsync();
            }

            await mediator.Publish(new TicketTagsRemovedNotification(request.TicketId));

            return Unit.Value;
        }
    }
}