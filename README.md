# TicketManager - .NET Event Sourcing and CQRS demo app
A demo ticket manager app which I develop to study and experiment with event sourcing and CQRS.

The app is a simplified ticket manager with which users (no authentication planned for now) can create tickets such as tasks and bugs, provide them with details, assign priorities, perform state transitions, comment on them and create links between them such as clone, duplicate, etc.

The app uses Entity Framework Core 2.1 and MS SQL Server for storing the events and RavenDb 4.1 for storing the contemporary state constructed from the events.
