# TicketManager - .NET Event Sourcing and CQRS demo app
A demo ticket manager app which I develop to study and experiment with event sourcing and CQRS.

The app is a simplified ticket manager in which users (no authentication planned for now) can create tickets such as tasks and bugs, provide them with details, assign priorities, perform state transitions, comment on them and create links between them such as clone, duplicate, etc.

The app uses [Entity Framework Core 2.2](https://github.com/aspnet/EntityFrameworkCore) and MS SQL Server for storing the events and [RavenDb 4.1](https://github.com/ravendb/ravendb) for storing the contemporary state constructed from the events as JSON documents.

The query and command APIs run in the same process for now, so there is no dependency on an external message bus, it uses [MediatR](https://github.com/jbogard/MediatR) to dispatch notifications when an event occurs and the documents must be updated.
