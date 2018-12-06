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
        public static string GeneratePrefixedDocumentId<TDocument>(this IDocumentStore store, long customIdValue)
        {
            var separator = store.Conventions.IdentityPartsSeparator;
            var collectionName = store.Conventions.GetCollectionName(typeof(TDocument));

            return string.Concat(collectionName, separator, customIdValue.ToString());
        }

        public static string TrimIdPrefix<TDocument>(this IDocumentStore store, string documentId)
        {
            var separator = store.Conventions.IdentityPartsSeparator;
            var collectionName = store.Conventions.GetCollectionName(typeof(TDocument));

            var prefix = collectionName + separator;
            if (!documentId.StartsWith(prefix))
            {
                throw new ArgumentException($"The parameter '{nameof(documentId)}' must start with '{prefix}' to be able to remove the prefix.", nameof(documentId));
            }

            return documentId.Substring(prefix.Length);
        }

        public static Task PatchToNewer<TDocument>(this IDocumentStore store, string id, PropertyUpdateBatch<TDocument> propertyUpdates, Expression<Func<TDocument, DateTime>> timestampSelector, DateTime utcUpdateDate)
        {
            return PatchToNewer(store, id, timestampSelector, utcUpdateDate, propertyUpdates.CreateBatch());
        }

        public static async Task PatchToNewer<TDocument>(this IDocumentStore store, string id, Expression<Func<TDocument, DateTime>> timestampSelector, DateTime utcUpdateDate, params PropertyUpdateDescriptor[] propertyUpdates)
        {
            const string argumentName = "__AUTO_DateUpdated";
            const string variableName = "shouldUpdate";

            var timestampPropertyPath = GetPropertyPathFromExpression(timestampSelector);
            var conditionVariableScript = CreateConditionVariableScript(argumentName, variableName, timestampPropertyPath);

            // +1 for the update timestamp which does not need to be provided explicitly.
            var assignmentScripts = new List<string>(propertyUpdates.Length + 1);
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

        public static Task PatchToNewer<TDocument>(this IDocumentStore store, string id, PropertyUpdateBatch<TDocument> propertyUpdates, Expression<Func<TDocument, long>> lastKnownChangeIdSelector, long lastKnownChangeId)
        {
            return PatchToNewer(store, id, lastKnownChangeIdSelector, lastKnownChangeId, propertyUpdates.CreateBatch());
        }

        public static async Task PatchToNewer<TDocument>(this IDocumentStore store, string id, Expression<Func<TDocument, long>> lastKnownChangeIdSelector, long lastKnownChangeId, params PropertyUpdateDescriptor[] propertyUpdates)
        {
            const string argumentName = "__AUTO_LastKnownChangeId";
            const string variableName = "shouldUpdate";

            var timestampPropertyPath = GetPropertyPathFromExpression(lastKnownChangeIdSelector);
            var conditionVariableScript = CreateConditionVariableScript(argumentName, variableName, timestampPropertyPath);

            // +1 for the last known change Id which does not need to be provided explicitly.
            var assignmentScripts = new List<string>(propertyUpdates.Length + 1);
            var scriptParameters = new ScriptParameterDictionary
            {
                [argumentName] = lastKnownChangeId
            };

            for (var i = 0; i < propertyUpdates.Length; ++i)
            {
                var propertyUpdate = propertyUpdates[i];
                var parameterName = string.Concat("__AUTO_Parameter", i);

                assignmentScripts.Add(CreateUpdateScript(propertyUpdate, scriptParameters, parameterName));
                scriptParameters.AddParameter(parameterName, propertyUpdate.NewValue);
            }

            assignmentScripts.Add("\t" + timestampPropertyPath + " = args.__AUTO_LastKnownChangeId;");

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

        private static string CreateConditionVariableScript(string argumentName, string variableName, string conditionPropertyPath)
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

        private static string GetPropertyPathFromExpression(LambdaExpression lambdaExpression)
        {
            // If the expression is something like doc => doc.LastUpdate.LastKnownChangeId, this returns [ "LastUpdate", "UtcDateUpdated" ], so it omits the "doc" parameter.
            var pathToTimestampProperty = ExpressionHelper.GetMemberList(lambdaExpression);
            pathToTimestampProperty.Insert(0, "this");

            return string.Join(".", pathToTimestampProperty);
        }
    }
}