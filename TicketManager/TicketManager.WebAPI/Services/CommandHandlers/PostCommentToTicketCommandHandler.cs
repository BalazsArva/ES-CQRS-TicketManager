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
    public class PostCommentToTicketCommandHandler : IRequestHandler<PostCommentToTicketCommand, int>
    {
        private readonly IMediator mediator;
        private readonly IEventsContextFactory eventsContextFactory;
        private readonly PostCommentToTicketCommandValidator postCommentToTicketCommandValidator;

        public PostCommentToTicketCommandHandler(IMediator mediator, IEventsContextFactory eventsContextFactory, PostCommentToTicketCommandValidator postCommentToTicketCommandValidator)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));
            this.postCommentToTicketCommandValidator = postCommentToTicketCommandValidator ?? throw new ArgumentNullException(nameof(postCommentToTicketCommandValidator));
        }

        public async Task<int> Handle(PostCommentToTicketCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await postCommentToTicketCommandValidator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            int commentId;
            using (var context = eventsContextFactory.CreateContext())
            {
                var ticketCommentPostedEvent = new TicketCommentPostedEvent
                {
                    CausedBy = request.RaisedByUser,
                    TicketCreatedEventId = request.TicketId
                };

                context.TicketCommentPostedEvents.Add(ticketCommentPostedEvent);
                context.TicketCommentEditedEvents.Add(new TicketCommentEditedEvent
                {
                    CausedBy = request.RaisedByUser,
                    CommentText = request.CommentText,
                    TicketCommentPostedEvent = ticketCommentPostedEvent
                });

                await context.SaveChangesAsync();

                commentId = ticketCommentPostedEvent.Id;
            }

            await mediator.Publish(new TicketCommentPostedNotification(commentId));

            return commentId;
        }
    }
}