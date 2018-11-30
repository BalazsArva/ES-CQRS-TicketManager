﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TicketManager.DataAccess.Events;

namespace TicketManager.DataAccess.Events.Migrations
{
    [DbContext(typeof(EventsContext))]
    partial class EventsContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.4-rtm-31024")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("TicketManager.DataAccess.Events.DataModel.TicketAssignedEvent", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("AssignedTo")
                        .HasMaxLength(256);

                    b.Property<string>("CausedBy")
                        .IsRequired()
                        .HasMaxLength(256);

                    b.Property<int>("TicketCreatedEventId");

                    b.Property<DateTime>("UtcDateRecorded");

                    b.HasKey("Id");

                    b.HasIndex("TicketCreatedEventId");

                    b.HasIndex("UtcDateRecorded");

                    b.ToTable("TicketAssignedEvents");
                });

            modelBuilder.Entity("TicketManager.DataAccess.Events.DataModel.TicketCommentEditedEvent", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("CausedBy")
                        .IsRequired()
                        .HasMaxLength(256);

                    b.Property<string>("CommentText")
                        .IsRequired();

                    b.Property<int>("TicketCommentPostedEventId");

                    b.Property<DateTime>("UtcDateRecorded");

                    b.HasKey("Id");

                    b.HasIndex("TicketCommentPostedEventId");

                    b.HasIndex("UtcDateRecorded");

                    b.ToTable("TicketCommentEditedEvents");
                });

            modelBuilder.Entity("TicketManager.DataAccess.Events.DataModel.TicketCommentPostedEvent", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("CausedBy")
                        .IsRequired()
                        .HasMaxLength(256);

                    b.Property<int>("TicketCreatedEventId");

                    b.Property<DateTime>("UtcDateRecorded");

                    b.HasKey("Id");

                    b.HasIndex("TicketCreatedEventId");

                    b.HasIndex("UtcDateRecorded");

                    b.ToTable("TicketCommentPostedEvents");
                });

            modelBuilder.Entity("TicketManager.DataAccess.Events.DataModel.TicketCreatedEvent", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("CausedBy")
                        .IsRequired()
                        .HasMaxLength(256);

                    b.Property<DateTime>("UtcDateRecorded");

                    b.HasKey("Id");

                    b.HasIndex("UtcDateRecorded");

                    b.ToTable("TicketCreatedEvents");
                });

            modelBuilder.Entity("TicketManager.DataAccess.Events.DataModel.TicketDescriptionChangedEvent", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("CausedBy")
                        .IsRequired()
                        .HasMaxLength(256);

                    b.Property<string>("Description");

                    b.Property<int>("TicketCreatedEventId");

                    b.Property<DateTime>("UtcDateRecorded");

                    b.HasKey("Id");

                    b.HasIndex("TicketCreatedEventId");

                    b.HasIndex("UtcDateRecorded");

                    b.ToTable("TicketDescriptionChangedEvents");
                });

            modelBuilder.Entity("TicketManager.DataAccess.Events.DataModel.TicketLinkChangedEvent", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("CausedBy")
                        .IsRequired()
                        .HasMaxLength(256);

                    b.Property<bool>("ConnectionIsActive");

                    b.Property<int>("LinkType");

                    b.Property<int>("SourceTicketCreatedEventId");

                    b.Property<int>("TargetTicketCreatedEventId");

                    b.Property<DateTime>("UtcDateRecorded");

                    b.HasKey("Id");

                    b.HasIndex("SourceTicketCreatedEventId");

                    b.HasIndex("TargetTicketCreatedEventId");

                    b.HasIndex("UtcDateRecorded");

                    b.ToTable("TicketLinkChangedEvents");
                });

            modelBuilder.Entity("TicketManager.DataAccess.Events.DataModel.TicketPriorityChangedEvent", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("CausedBy")
                        .IsRequired()
                        .HasMaxLength(256);

                    b.Property<int>("Priority");

                    b.Property<int>("TicketCreatedEventId");

                    b.Property<DateTime>("UtcDateRecorded");

                    b.HasKey("Id");

                    b.HasIndex("TicketCreatedEventId");

                    b.HasIndex("UtcDateRecorded");

                    b.ToTable("TicketPriorityChangedEvents");
                });

            modelBuilder.Entity("TicketManager.DataAccess.Events.DataModel.TicketStatusChangedEvent", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("CausedBy")
                        .IsRequired()
                        .HasMaxLength(256);

                    b.Property<int>("TicketCreatedEventId");

                    b.Property<int>("TicketStatus");

                    b.Property<DateTime>("UtcDateRecorded");

                    b.HasKey("Id");

                    b.HasIndex("TicketCreatedEventId");

                    b.HasIndex("UtcDateRecorded");

                    b.ToTable("TicketStatusChangedEvents");
                });

            modelBuilder.Entity("TicketManager.DataAccess.Events.DataModel.TicketTagChangedEvent", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("CausedBy")
                        .IsRequired()
                        .HasMaxLength(256);

                    b.Property<string>("Tag")
                        .IsRequired()
                        .HasMaxLength(64);

                    b.Property<bool>("TagAdded");

                    b.Property<int>("TicketCreatedEventId");

                    b.Property<DateTime>("UtcDateRecorded");

                    b.HasKey("Id");

                    b.HasIndex("TicketCreatedEventId");

                    b.HasIndex("UtcDateRecorded");

                    b.ToTable("TicketTagChangedEvents");
                });

            modelBuilder.Entity("TicketManager.DataAccess.Events.DataModel.TicketTitleChangedEvent", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("CausedBy")
                        .IsRequired()
                        .HasMaxLength(256);

                    b.Property<int>("TicketCreatedEventId");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(256);

                    b.Property<DateTime>("UtcDateRecorded");

                    b.HasKey("Id");

                    b.HasIndex("TicketCreatedEventId");

                    b.HasIndex("UtcDateRecorded");

                    b.ToTable("TicketTitleChangedEvents");
                });

            modelBuilder.Entity("TicketManager.DataAccess.Events.DataModel.TicketTypeChangedEvent", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("CausedBy")
                        .IsRequired()
                        .HasMaxLength(256);

                    b.Property<int>("TicketCreatedEventId");

                    b.Property<int>("TicketType");

                    b.Property<DateTime>("UtcDateRecorded");

                    b.HasKey("Id");

                    b.HasIndex("TicketCreatedEventId");

                    b.HasIndex("UtcDateRecorded");

                    b.ToTable("TicketTypeChangedEvents");
                });

            modelBuilder.Entity("TicketManager.DataAccess.Events.DataModel.TicketAssignedEvent", b =>
                {
                    b.HasOne("TicketManager.DataAccess.Events.DataModel.TicketCreatedEvent", "TicketCreatedEvent")
                        .WithMany()
                        .HasForeignKey("TicketCreatedEventId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("TicketManager.DataAccess.Events.DataModel.TicketCommentEditedEvent", b =>
                {
                    b.HasOne("TicketManager.DataAccess.Events.DataModel.TicketCommentPostedEvent", "TicketCommentPostedEvent")
                        .WithMany()
                        .HasForeignKey("TicketCommentPostedEventId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("TicketManager.DataAccess.Events.DataModel.TicketCommentPostedEvent", b =>
                {
                    b.HasOne("TicketManager.DataAccess.Events.DataModel.TicketCreatedEvent", "TicketCreatedEvent")
                        .WithMany()
                        .HasForeignKey("TicketCreatedEventId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("TicketManager.DataAccess.Events.DataModel.TicketDescriptionChangedEvent", b =>
                {
                    b.HasOne("TicketManager.DataAccess.Events.DataModel.TicketCreatedEvent", "TicketCreatedEvent")
                        .WithMany()
                        .HasForeignKey("TicketCreatedEventId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("TicketManager.DataAccess.Events.DataModel.TicketLinkChangedEvent", b =>
                {
                    b.HasOne("TicketManager.DataAccess.Events.DataModel.TicketCreatedEvent", "SourceTicketCreatedEvent")
                        .WithMany()
                        .HasForeignKey("SourceTicketCreatedEventId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("TicketManager.DataAccess.Events.DataModel.TicketCreatedEvent", "TargetTicketCreatedEvent")
                        .WithMany()
                        .HasForeignKey("TargetTicketCreatedEventId")
                        .OnDelete(DeleteBehavior.Restrict);
                });

            modelBuilder.Entity("TicketManager.DataAccess.Events.DataModel.TicketPriorityChangedEvent", b =>
                {
                    b.HasOne("TicketManager.DataAccess.Events.DataModel.TicketCreatedEvent", "TicketCreatedEvent")
                        .WithMany()
                        .HasForeignKey("TicketCreatedEventId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("TicketManager.DataAccess.Events.DataModel.TicketStatusChangedEvent", b =>
                {
                    b.HasOne("TicketManager.DataAccess.Events.DataModel.TicketCreatedEvent", "TicketCreatedEvent")
                        .WithMany()
                        .HasForeignKey("TicketCreatedEventId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("TicketManager.DataAccess.Events.DataModel.TicketTagChangedEvent", b =>
                {
                    b.HasOne("TicketManager.DataAccess.Events.DataModel.TicketCreatedEvent", "TicketCreatedEvent")
                        .WithMany()
                        .HasForeignKey("TicketCreatedEventId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("TicketManager.DataAccess.Events.DataModel.TicketTitleChangedEvent", b =>
                {
                    b.HasOne("TicketManager.DataAccess.Events.DataModel.TicketCreatedEvent", "TicketCreatedEvent")
                        .WithMany()
                        .HasForeignKey("TicketCreatedEventId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("TicketManager.DataAccess.Events.DataModel.TicketTypeChangedEvent", b =>
                {
                    b.HasOne("TicketManager.DataAccess.Events.DataModel.TicketCreatedEvent", "TicketCreatedEvent")
                        .WithMany()
                        .HasForeignKey("TicketCreatedEventId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
