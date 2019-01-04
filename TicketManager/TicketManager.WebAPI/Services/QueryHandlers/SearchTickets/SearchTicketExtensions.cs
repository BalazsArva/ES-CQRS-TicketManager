using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using FluentValidation;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using TicketManager.Common.Utils;
using TicketManager.Contracts.QueryApi;
using TicketManager.Contracts.QueryApi.Models;
using TicketManager.DataAccess.Documents.DataModel;
using TicketManager.WebAPI.Conventions;
using TicketManager.WebAPI.DTOs.Queries;

namespace TicketManager.WebAPI.Services.QueryHandlers.SearchTickets
{
    public static class SearchTicketExtensions
    {
        private static readonly MethodInfo StringStartsWithMethodInfo;
        private static readonly MethodInfo StringEndsWithMethodInfo;

        static SearchTicketExtensions()
        {
            StringEndsWithMethodInfo = typeof(string).GetMethod("EndsWith", new[] { typeof(string) });
            StringStartsWithMethodInfo = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
        }

        public static IRavenQueryable<Ticket> ApplyFilters(this IRavenQueryable<Ticket> queryIn, SearchTicketsQueryRequest request)
        {
            var query = queryIn
                .SearchForCreatedBy(request.CreatedBy)
                .SearchForLastUpdatedBy(request.LastModifiedBy)
                .SearchForTitle(request.Title)
                .SearchForAssignedTo(request.AssignedTo)
                .SearchForDateCreatedFrom(request.DateCreatedFrom)
                .SearchForDateCreatedTo(request.DateCreatedTo)
                .SearchForDateLastUpdatedFrom(request.DateLastModifiedFrom)
                .SearchForDateLastUpdatedTo(request.DateLastModifiedTo)
                .SearchForTicketPriority(request.Priority)
                .SearchForTicketStatus(request.Status)
                .SearchForTicketType(request.TicketType)
                .SearchForTags(request.Tags);

            foreach (var involvedUser in request.InvolvedUsers)
            {
                query = query.Where(t => t.Involvement.InvolvedUsersSet.Any(user => user == involvedUser));
            }

            return query;
        }

        public static IRavenQueryable<Ticket> ApplySortings(this IRavenQueryable<Ticket> query, SearchTicketsQueryRequest request)
        {
            var orderBy = Enum.Parse<SearchTicketsOrderByProperty>(request.OrderBy, true);
            var orderDirection = Enum.Parse<OrderDirection>(request.OrderDirection, true);

            switch (orderBy)
            {
                case SearchTicketsOrderByProperty.CreatedBy:
                    return orderDirection == OrderDirection.Ascending
                        ? query.OrderBy(t => t.CreatedBy)
                        : query.OrderByDescending(t => t.CreatedBy);

                case SearchTicketsOrderByProperty.Id:
                    return orderDirection == OrderDirection.Ascending
                        ? query.OrderBy(t => t.Id)
                        : query.OrderByDescending(t => t.Id);

                case SearchTicketsOrderByProperty.LastModifiedBy:
                    return orderDirection == OrderDirection.Ascending
                        ? query.OrderBy(t => t.LastUpdatedBy)
                        : query.OrderByDescending(t => t.LastUpdatedBy);

                case SearchTicketsOrderByProperty.Priority:
                    return orderDirection == OrderDirection.Ascending
                        ? query.OrderBy(t => t.TicketPriority.Priority.ToString())
                        : query.OrderByDescending(t => t.TicketPriority.Priority.ToString());

                case SearchTicketsOrderByProperty.Status:
                    return orderDirection == OrderDirection.Ascending
                        ? query.OrderBy(t => t.TicketStatus.Status.ToString())
                        : query.OrderByDescending(t => t.TicketStatus.Status.ToString());

                case SearchTicketsOrderByProperty.Title:
                    return orderDirection == OrderDirection.Ascending
                        ? query.OrderBy(t => t.TicketTitle.Title)
                        : query.OrderByDescending(t => t.TicketTitle.Title);

                case SearchTicketsOrderByProperty.AssignedTo:
                    return orderDirection == OrderDirection.Ascending
                        ? query.OrderBy(t => t.Assignment.AssignedTo)
                        : query.OrderByDescending(t => t.Assignment.AssignedTo);

                case SearchTicketsOrderByProperty.Type:
                    return orderDirection == OrderDirection.Ascending
                        ? query.OrderBy(t => t.TicketType.Type.ToString())
                        : query.OrderByDescending(t => t.TicketType.Type.ToString());

                case SearchTicketsOrderByProperty.UtcDateCreated:
                    return orderDirection == OrderDirection.Ascending
                        ? query.OrderBy(t => t.UtcDateCreated)
                        : query.OrderByDescending(t => t.UtcDateCreated);

                case SearchTicketsOrderByProperty.UtcDateLastModified:
                    return orderDirection == OrderDirection.Ascending
                        ? query.OrderBy(t => t.UtcDateLastUpdated)
                        : query.OrderByDescending(t => t.UtcDateLastUpdated);

                // Cannot happen as the validator prevents it and the Enum.Parse would also fail for an invalid value.
                default:
                    throw new NotSupportedException();
            }
        }

        public static IRavenQueryable<Ticket> SearchForCreatedBy(this IRavenQueryable<Ticket> query, string createdBy)
        {
            return query.SearchForStringFieldValue(t => t.CreatedBy, createdBy);
        }

        public static IRavenQueryable<Ticket> SearchForLastUpdatedBy(this IRavenQueryable<Ticket> query, string lastUpdatedBy)
        {
            return query.SearchForStringFieldValue(t => t.LastUpdatedBy, lastUpdatedBy);
        }

        public static IRavenQueryable<Ticket> SearchForTitle(this IRavenQueryable<Ticket> query, string title)
        {
            return query.SearchForStringFieldValue(t => t.TicketTitle.Title, title);
        }

        public static IRavenQueryable<Ticket> SearchForAssignedTo(this IRavenQueryable<Ticket> query, string assignedTo)
        {
            return query.SearchForStringFieldValue(t => t.Assignment.AssignedTo, assignedTo);
        }

        public static IRavenQueryable<Ticket> SearchForDateCreatedFrom(this IRavenQueryable<Ticket> query, string dateCreatedFrom)
        {
            if (!string.IsNullOrEmpty(dateCreatedFrom))
            {
                var utcDateCreatedFrom = DateTimeConventions.GetUniversalDateTime(dateCreatedFrom);

                return query.Where(t => t.UtcDateCreated >= utcDateCreatedFrom);
            }

            return query;
        }

        public static IRavenQueryable<Ticket> SearchForDateCreatedTo(this IRavenQueryable<Ticket> query, string dateCreatedTo)
        {
            if (!string.IsNullOrEmpty(dateCreatedTo))
            {
                var utcDateCreatedTo = DateTimeConventions.GetUniversalDateTime(dateCreatedTo);

                return query.Where(t => t.UtcDateCreated <= utcDateCreatedTo);
            }

            return query;
        }

        public static IRavenQueryable<Ticket> SearchForDateLastUpdatedFrom(this IRavenQueryable<Ticket> query, string dateLastUpdatedFrom)
        {
            if (!string.IsNullOrEmpty(dateLastUpdatedFrom))
            {
                var utcDateLastUpdatedFrom = DateTimeConventions.GetUniversalDateTime(dateLastUpdatedFrom);

                return query.Where(t => t.UtcDateLastUpdated >= utcDateLastUpdatedFrom);
            }

            return query;
        }

        public static IRavenQueryable<Ticket> SearchForDateLastUpdatedTo(this IRavenQueryable<Ticket> query, string dateLastUpdatedTo)
        {
            if (!string.IsNullOrEmpty(dateLastUpdatedTo))
            {
                var utcDateLastUpdatedTo = DateTimeConventions.GetUniversalDateTime(dateLastUpdatedTo);

                return query.Where(t => t.UtcDateLastUpdated <= utcDateLastUpdatedTo);
            }

            return query;
        }

        public static IRavenQueryable<Ticket> SearchForTicketPriority(this IRavenQueryable<Ticket> query, string priority)
        {
            return query.SearchForEnumFieldValue(t => t.TicketPriority.Priority, priority);
        }

        public static IRavenQueryable<Ticket> SearchForTicketType(this IRavenQueryable<Ticket> query, string type)
        {
            return query.SearchForEnumFieldValue(t => t.TicketType.Type, type);
        }

        public static IRavenQueryable<Ticket> SearchForTicketStatus(this IRavenQueryable<Ticket> query, string status)
        {
            return query.SearchForEnumFieldValue(t => t.TicketStatus.Status, status);
        }

        public static IRavenQueryable<Ticket> SearchForTags(this IRavenQueryable<Ticket> query, IEnumerable<string> tags)
        {
            foreach (var tag in tags ?? Array.Empty<string>())
            {
                var (kind, transformedValue) = StringSearchHelper.ParseSearchSyntax(tag);

                if (kind == StringSearchKind.EndsWith)
                {
                    query = query.Where(ticket => ticket.Tags.TagSet.Any(t => t.EndsWith(transformedValue)));
                }
                else if (kind == StringSearchKind.Equals)
                {
                    query = query.Where(ticket => ticket.Tags.TagSet.Any(t => t == transformedValue));
                }
                else if (kind == StringSearchKind.StartsWith)
                {
                    query = query.Where(ticket => ticket.Tags.TagSet.Any(t => t.StartsWith(transformedValue)));
                }
                else
                {
                    // TODO: This does not work. Fix it.
                    query = query.Search(t => t.Tags.TagSet, transformedValue);
                }
            }

            return query;
        }

        private static IRavenQueryable<Ticket> SearchForStringFieldValue(this IRavenQueryable<Ticket> query, Expression<Func<Ticket, string>> fieldSelector, string searchQuery)
        {
            if (!string.IsNullOrEmpty(searchQuery))
            {
                var (searchKind, searchValue) = StringSearchHelper.ParseSearchSyntax(searchQuery);

                var memberExpression = fieldSelector.Body as MemberExpression;
                var comparisonValueExpression = Expression.Constant(searchValue);

                if (searchKind == StringSearchKind.EndsWith)
                {
                    var endsWithMethodInvocationExpression = Expression.Call(
                        memberExpression,
                        StringEndsWithMethodInfo,
                        comparisonValueExpression);

                    var endsWithLambdaExpression = Expression.Lambda<Func<Ticket, bool>>(endsWithMethodInvocationExpression, fieldSelector.Parameters);

                    return query.Where(endsWithLambdaExpression);
                }

                if (searchKind == StringSearchKind.Equals)
                {
                    var equalityExpression = Expression.Equal(memberExpression, comparisonValueExpression);
                    var equalsLambdaExpression = Expression.Lambda<Func<Ticket, bool>>(equalityExpression, fieldSelector.Parameters);

                    return query.Where(equalsLambdaExpression);
                }

                if (searchKind == StringSearchKind.StartsWith)
                {
                    var startsWithMethodInvocationExpression = Expression.Call(
                        memberExpression,
                        StringStartsWithMethodInfo,
                        comparisonValueExpression);

                    var startsWithLambdaExpression = Expression.Lambda<Func<Ticket, bool>>(startsWithMethodInvocationExpression, fieldSelector.Parameters);

                    return query.Where(startsWithLambdaExpression);
                }

                var searchFieldSelector = Expression.Lambda<Func<Ticket, object>>(fieldSelector.Body, fieldSelector.Parameters);

                return query.Search(searchFieldSelector, searchValue);
            }

            return query;
        }

        private static IRavenQueryable<Ticket> SearchForEnumFieldValue<TEnum>(this IRavenQueryable<Ticket> query, Expression<Func<Ticket, TEnum>> fieldSelector, string searchFieldValue)
            where TEnum : struct, Enum
        {
            if (!string.IsNullOrEmpty(searchFieldValue))
            {
                var enumValue = Enum.Parse<TEnum>(searchFieldValue, true);

                var memberExpression = fieldSelector.Body as MemberExpression;
                var comparisonValueExpression = Expression.Constant(enumValue);
                var equalityExpression = Expression.Equal(memberExpression, comparisonValueExpression);
                var equalsLambdaExpression = Expression.Lambda<Func<Ticket, bool>>(equalityExpression, fieldSelector.Parameters);

                return query.Where(equalsLambdaExpression);
            }

            return query;
        }
    }
}