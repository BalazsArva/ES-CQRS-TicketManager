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
    public class AddTicketTagsCommandHandler : IRequestHandler<AddTicketTagsCommand>
    {
        private readonly IMediator mediator;
        private readonly IEventsContextFactory eventsContextFactory;
        private readonly IValidator<AddTicketTagsCommand> validator;

        public AddTicketTagsCommandHandler(IMediator mediator, IEventsContextFactory eventsContextFactory, IValidator<AddTicketTagsCommand> validator)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));
            this.validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public async Task<Unit> Handle(AddTicketTagsCommand request, CancellationToken cancellationToken)
        {
            await validator.ValidateAndThrowAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);

            using (var context = eventsContextFactory.CreateContext())
            {
                foreach (var tag in request.Tags)
                {
                    context.TicketTagChangedEvents.Add(new TicketTagChangedEvent
                    {
                        CausedBy = request.RaisedByUser,
                        Tag = tag,
                        TagAdded = true,
                        TicketCreatedEventId = request.TicketId
                    });
                }

                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            await mediator.Publish(new TicketTagsAddedNotification(request.TicketId), cancellationToken).ConfigureAwait(false);

            return Unit.Value;
        }
    }
}