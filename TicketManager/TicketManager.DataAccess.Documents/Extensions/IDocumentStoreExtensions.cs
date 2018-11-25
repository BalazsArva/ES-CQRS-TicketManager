using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Raven.Client.Documents;
using Raven.Client.Documents.Operations;
using TicketManager.Common.Linq.Expressions;
using TicketManager.Common.Utils;
using TicketManager.DataAccess.Documents.DataStructures;

namespace TicketManager.DataAccess.Documents.Extensions
{
    public static class IDocumentStoreExtensions
    {
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
                var newValue = propertyUpdates[i].NewValue;
                var paramName = "__AUTO_Parameter" + i.ToString();

                assignmentScripts.Add("\t" + propertyUpdates[i].ToJavaScriptPropertyExpression() + " = args." + paramName + ";");

                if (ObjectHelper.IsCollection(newValue))
                {
                    parameters[paramName] = JArray.FromObject(newValue);
                }
                else if (ObjectHelper.IsPrimitive(newValue) || ObjectHelper.IsDateTimeLike(newValue))
                {
                    parameters[paramName] = newValue;
                }
                else
                {
                    throw new NotSupportedException($"The patch operation could not handle the value of type '{newValue.GetType().FullName}'.");
                }
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