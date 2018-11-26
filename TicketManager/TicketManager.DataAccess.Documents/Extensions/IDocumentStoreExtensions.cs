using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.Documents.Operations;
using TicketManager.Common.Linq.Expressions;
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
            const string argumentName = "__AUTO_DateUpdated";
            const string variableName = "shouldUpdate";

            var timestampPropertyPath = GetTimestampPropertyPath(timestampSelector);
            var conditionVariableScript = CreateConditionVariableScript(timestampSelector, utcUpdateDate, argumentName, variableName, timestampPropertyPath);

            var assignmentScripts = new List<string>(propertyUpdates.Length);
            var scriptParameters = new ScriptParameterDictionary
            {
                [argumentName] = utcUpdateDate
            };

            for (var i = 0; i < propertyUpdates.Length; ++i)
            {
                var propertyUpdate = propertyUpdates[i];
                var parameterName = string.Concat("__AUTO_Parameter", i);

                assignmentScripts.Add(CreateUpdateScript(propertyUpdate, scriptParameters, parameterName));
                scriptParameters.AddParameter(parameterName, propertyUpdate.NewValue);
            }

            assignmentScripts.Add("\t" + timestampPropertyPath + " = args.__AUTO_DateUpdated;");

            var script = string.Join(
                Environment.NewLine,
                conditionVariableScript,
                $"if ({variableName})",
                "{",
                string.Join(Environment.NewLine, assignmentScripts),
                "}");

            var patchRequest = new PatchRequest
            {
                Script = script,
                Values = scriptParameters
            };

            await store.Operations.SendAsync(new PatchOperation(id, null, patchRequest));
        }

        private static string CreateUpdateScript(PropertyUpdateDescriptor propertyUpdateDescriptor, Dictionary<string, object> parameters, string parameterName)
        {
            var returnValue = new StringBuilder();

            returnValue.Append("\t");
            returnValue.Append(propertyUpdateDescriptor.ToJavaScriptPropertyExpression());
            returnValue.Append(" = args.");
            returnValue.Append(parameterName);
            returnValue.Append(";");

            return returnValue.ToString();
        }

        private static string CreateConditionVariableScript<TDocument>(Expression<Func<TDocument, DateTime>> timestampSelector, DateTime utcUpdateDate, string argumentName, string variableName, string conditionPropertyPath)
        {
            var returnValue = new StringBuilder();

            returnValue.Append("var ");
            returnValue.Append(variableName);
            returnValue.Append(" = ");
            returnValue.Append(conditionPropertyPath);
            returnValue.Append(" < args.");
            returnValue.Append(argumentName);
            returnValue.Append(";");

            return returnValue.ToString();
        }

        private static string GetTimestampPropertyPath<TDocument>(Expression<Func<TDocument, DateTime>> timestampSelector)
        {
            // If the expression is something like doc => doc.LastUpdate.UtcDateUpdated, this returns [ "LastUpdate", "UtcDateUpdated" ], so it omits the "doc" parameter.
            var pathToTimestampProperty = ExpressionHelper.GetMemberList(timestampSelector);
            pathToTimestampProperty.Insert(0, "this");

            return string.Join(".", pathToTimestampProperty);
        }
    }
}