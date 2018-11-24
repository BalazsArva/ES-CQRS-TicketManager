using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.Documents.Operations;
using TicketManager.Common.Linq.Expressions;
using TicketManager.DataAccess.Documents.DataStructures;

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

        public static Task PatchToNewer<TDocument>(this IDocumentStore store, string id, PropertyUpdateBatch<TDocument> propertyUpdates, Expression<Func<TDocument, DateTime>> timestampSelector, DateTime utcUpdateDate)
        {
            return PatchToNewer(store, id, timestampSelector, utcUpdateDate, propertyUpdates.CreateBatch());
        }

        public static async Task PatchToNewer<TDocument>(this IDocumentStore store, string id, Expression<Func<TDocument, DateTime>> timestampSelector, DateTime utcUpdateDate, params PropertyUpdateDescriptor[] propertyUpdates)
        {
            // If the expression is something like doc => doc.LastUpdate.UtcDateUpdated, this returns [ "LastUpdate", "UtcDateUpdated" ], so it omits the "doc" parameter.
            var pathToTimestampProperty = ExpressionHelper.GetMemberList(timestampSelector);
            pathToTimestampProperty.Insert(0, "this");

            var conditionPropertyPath = string.Join(".", pathToTimestampProperty);
            var updateConditionScript = string.Concat(
                "var shouldUpdate = ",
                conditionPropertyPath,
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

            assignmentScripts.Add("\t" + conditionPropertyPath + " = args.__AUTO_DateUpdated;");

            var script = string.Join(
                Environment.NewLine,
                updateConditionScript,
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
}