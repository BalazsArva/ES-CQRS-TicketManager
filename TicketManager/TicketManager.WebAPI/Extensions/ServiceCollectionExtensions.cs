﻿using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raven.Client.Documents;
using TicketManager.DataAccess.Events;
using TicketManager.WebAPI.DTOs;
using TicketManager.WebAPI.DTOs.Commands;
using TicketManager.WebAPI.Validation.CommandValidators;

namespace TicketManager.WebAPI.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRavenDb(this IServiceCollection services, IConfiguration configuration)
        {
            var ravenDbUrls = configuration.GetSection("DataAccess:RavenDb:Urls").Get<string[]>();
            var ravenDbDatabase = configuration["DataAccess:RavenDb:Database"];

            services.AddSingleton(new DocumentStore
            {
                Urls = ravenDbUrls,
                Database = ravenDbDatabase
            }.Initialize());

            return services;
        }

        public static IServiceCollection AddEventsContext(this IServiceCollection services, IConfiguration Configuration)
        {
            var sqlConnectionString = Configuration["DataAccess:SQL"];

            var options = new DbContextOptionsBuilder<EventsContext>()
                .UseSqlServer(sqlConnectionString)
                .Options;

            services.AddSingleton(options);
            services.AddDbContext<EventsContext>(opts => opts.UseSqlServer(sqlConnectionString));
            services.AddSingleton<IEventsContextFactory>(svcProvider =>
            {
                return new EventsContextFactory(options);
            });

            return services;
        }

        public static IServiceCollection AddValidators(this IServiceCollection services)
        {
            return services
                .AddSingleton<IValidator<AddTicketLinksCommand>, AddTicketLinksCommandValidator>()
                .AddSingleton<IValidator<AddTicketTagsCommand>, AddTicketTagsCommandValidator>()
                .AddSingleton<IValidator<AssignTicketCommand>, AssignTicketCommandValidator>()
                .AddSingleton<IValidator<ChangeTicketPriorityCommand>, ChangeTicketPriorityCommandValidator>()
                .AddSingleton<IValidator<ChangeTicketStatusCommand>, ChangeTicketStatusCommandValidator>()
                .AddSingleton<IValidator<ChangeTicketTypeCommand>, ChangeTicketTypeCommandValidator>()
                .AddSingleton<IValidator<CreateTicketCommand>, CreateTicketCommandValidator>()
                .AddSingleton<IValidator<EditTicketCommentCommand>, EditTicketCommentCommandValidator>()
                .AddSingleton<IValidator<EditTicketTitleCommand>, EditTicketTitleCommandValidator>()
                .AddSingleton<IValidator<EditTicketDescriptionCommand>, EditTicketDescriptionCommandValidator>()
                .AddSingleton<IValidator<PostCommentToTicketCommand>, PostCommentToTicketCommandValidator>()
                .AddSingleton<IValidator<RemoveTicketLinksCommand>, RemoveTicketLinksCommandValidator>()
                .AddSingleton<IValidator<RemoveTicketTagsCommand>, RemoveTicketTagsCommandValidator>()
                .AddSingleton<IValidator<UpdateTicketCommand>, UpdateTicketCommandValidator>()
                .AddSingleton<IValidator<TicketLinkDTO>, TicketLinkValidator>();
        }
    }
}