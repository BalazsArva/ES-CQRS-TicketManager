using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.Documents.Operations;

namespace TicketManager.DataAccess.Documents.Extensions
{
    public static class IDocumentStoreExtensions
    {
        /*
        public static async Task PatchConditionally<TDocument>(this IDocumentStore store, Expression<Func<TDocument, bool>> condition)
        {
            // condition example:
            // doc => doc.LastUpdate.UtcDateUpdated
            var conditionParameterName = condition.Parameters[0];
            //condition.Body.

            var patchRequest = new PatchRequest
            {
                Script = "",
                Values =
                {
                    ["DateUpdated"] = utcDateUpdated,
                    ["AssignedBy"] = assigner,
                    ["AssignedTo"] = assignedTo,
                }
            };

            await store.Operations.SendAsync(new PatchOperation(id, null, patchRequest));
        }
        */

        public static async Task PatchToNewer<TDocument>(this IDocumentStore store, string id, Expression<Func<TDocument, DateTime>> timestampSelector, DateTime utcUpdateDate,
            params PropertyUpdate[] propertyUpdates)
        {
            var members = ExpressionHelper.GetMemberList((MemberExpression)timestampSelector.Body);
            members.Insert(0, "this");

            var jsPropertyExpression = string.Join(".", members);
            var jsCondition = string.Concat(
                "var shouldUpdate = ",
                jsPropertyExpression,
                " < args.__AUTO_DateUpdated;");

            var assignmentScripts = new List<string>(propertyUpdates.Length);
            var parameters = new Dictionary<string, object>
            {
                ["__AUTO_DateUpdated"] = utcUpdateDate
            };

            for (var i = 0; i < propertyUpdates.Length; ++i)
            {
                var paramName = "__AUTO_Parameter" + i.ToString();

                assignmentScripts.Add("\t" + propertyUpdates[i].ToJavaScriptPropertyExpression() + " = args." + paramName + ";");
                parameters[paramName] = propertyUpdates[i].NewValue;
            }

            var script = string.Join(
                Environment.NewLine,
                jsCondition,
                "if (shouldUpdate) {",
                string.Join(Environment.NewLine, assignmentScripts),
                "}");

            var patchRequest = new PatchRequest
            {
                Script = script,
                Values = parameters
            };

            await store.Operations.SendAsync(new PatchOperation(id, null, patchRequest));
        }
    }

    public static class ExpressionHelper
    {
        public static List<string> GetMemberList(MemberExpression expression)
        {
            var result = new List<string>();

            if (expression.Expression is MemberExpression memberExpression)
            {
                result.AddRange(GetMemberList(memberExpression));
            }

            result.Add(expression.Member.Name);

            return result;
        }
    }

    public abstract class PropertyUpdate
    {
        protected PropertyUpdate(object newValue)
        {
            NewValue = newValue;
        }

        public object NewValue { get; }

        public abstract string ToJavaScriptPropertyExpression();
    }

    public class PropertyUpdate<TObject, TProperty> : PropertyUpdate
    {
        private readonly List<string> memberList;

        public PropertyUpdate(Expression<Func<TObject, TProperty>> memberSelector, TProperty newValue)
            : base(newValue)
        {
            if (memberSelector.Body is MemberExpression memberExpression)
            {
                memberList = ExpressionHelper.GetMemberList(memberExpression);
            }
            else
            {
                throw new ArgumentException($"The body of the {nameof(memberSelector)} parameter must be a {nameof(MemberExpression)}.", nameof(memberSelector));
            }
        }

        public override string ToJavaScriptPropertyExpression()
        {
            // TODO: Check that the memberlist has at least 1 element.
            return "this." + string.Join(".", memberList);
        }
    }
}