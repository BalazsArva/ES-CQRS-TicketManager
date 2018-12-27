using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Raven.Client.Documents.Commands.Batches;
using Raven.Client.Documents.Operations;
using Raven.Client.Documents.Session;
using TicketManager.Common.Linq.Expressions;
using TicketManager.DataAccess.Documents.DataStructures;

namespace TicketManager.DataAccess.Documents.Extensions
{
    public static class IAsyncDocumentSessionExtensions
    {
        public static void PatchToNewer<TDocument>(this IAsyncDocumentSession session, string id, PropertyUpdateBatch<TDocument> propertyUpdates, Expression<Func<TDocument, long>> lastKnownChangeIdSelector, long lastKnownChangeId)
        {
            var lastKnownChangeUpdateDescriptor = new PropertyUpdateDescriptor<TDocument, long>(lastKnownChangeIdSelector, lastKnownChangeId);
            var batch = propertyUpdates.CreateBatch().Append(lastKnownChangeUpdateDescriptor).ToArray();

            var patchRequest = CreatePatchRequest(lastKnownChangeIdSelector, lastKnownChangeId, batch);
            var patchCommandData = new PatchCommandData(id, null, patchRequest, null);

            session.Advanced.Defer(new ICommandData[] { patchCommandData });
        }

        public static void PatchToNewer<TDocument>(this IAsyncDocumentSession session, string id, PropertyUpdateBatch<TDocument> propertyUpdates, Expression<Func<TDocument, DateTime>> lastKnownChangeSelector, DateTime lastKnownChange)
        {
            var lastKnownChangeUpdateDescriptor = new PropertyUpdateDescriptor<TDocument, DateTime>(lastKnownChangeSelector, lastKnownChange);
            var batch = propertyUpdates.CreateBatch().Append(lastKnownChangeUpdateDescriptor).ToArray();

            var patchRequest = CreatePatchRequest(lastKnownChangeSelector, lastKnownChange, batch);
            var patchCommandData = new PatchCommandData(id, null, patchRequest, null);

            session.Advanced.Defer(new ICommandData[] { patchCommandData });
        }

        private static PatchRequest CreatePatchRequest<TDocument>(Expression<Func<TDocument, long>> lastKnownChangeIdSelector, long lastKnownChangeId, PropertyUpdateDescriptor[] propertyUpdates)
        {
            const string lastKnownChangeIdArgumentName = "__AUTO_LastKnownChangeId";
            const string conditionVariableName = "shouldUpdate";

            var lastKnownChangeIdPropertyPath = GetPropertyPathFromExpression(lastKnownChangeIdSelector);
            var conditionVariableScript = CreateConditionVariableScript(lastKnownChangeIdArgumentName, conditionVariableName, lastKnownChangeIdPropertyPath);

            var assignmentScripts = new List<string>(propertyUpdates.Length);
            var scriptParameters = new Dictionary<string, object>
            {
                [lastKnownChangeIdArgumentName] = lastKnownChangeId
            };

            for (var i = 0; i < propertyUpdates.Length; ++i)
            {
                var propertyUpdate = propertyUpdates[i];
                var parameterName = $"__AUTO_Parameter{i}";

                assignmentScripts.Add(CreateUpdateScript(propertyUpdate, parameterName));
                scriptParameters.Add(parameterName, propertyUpdate.NewValue);
            }

            var script = string.Join(
                Environment.NewLine,
                conditionVariableScript,
                $"if ({conditionVariableName})",
                "{",
                string.Join(Environment.NewLine, assignmentScripts),
                "}");

            var patchRequest = new PatchRequest
            {
                Script = script,
                Values = scriptParameters
            };

            return patchRequest;
        }

        private static PatchRequest CreatePatchRequest<TDocument>(Expression<Func<TDocument, DateTime>> lastKnownChangeSelector, DateTime lastKnownChange, PropertyUpdateDescriptor[] propertyUpdates)
        {
            const string lastKnownChangeArgumentName = "__AUTO_LastKnownChange";
            const string conditionVariableName = "shouldUpdate";

            var timestampPropertyPath = GetPropertyPathFromExpression(lastKnownChangeSelector);
            var conditionVariableScript = CreateConditionVariableScript(lastKnownChangeArgumentName, conditionVariableName, timestampPropertyPath);

            var assignmentScripts = new List<string>(propertyUpdates.Length);
            var scriptParameters = new Dictionary<string, object>
            {
                [lastKnownChangeArgumentName] = lastKnownChange
            };

            for (var i = 0; i < propertyUpdates.Length; ++i)
            {
                var propertyUpdate = propertyUpdates[i];
                var parameterName = $"__AUTO_Parameter{i}";

                assignmentScripts.Add(CreateUpdateScript(propertyUpdate, parameterName));
                scriptParameters.Add(parameterName, propertyUpdate.NewValue);
            }

            var script = string.Join(
                Environment.NewLine,
                conditionVariableScript,
                $"if ({conditionVariableName})",
                "{",
                string.Join(Environment.NewLine, assignmentScripts),
                "}");

            var patchRequest = new PatchRequest
            {
                Script = script,
                Values = scriptParameters
            };

            return patchRequest;
        }

        private static string CreateUpdateScript(PropertyUpdateDescriptor propertyUpdateDescriptor, string parameterName)
        {
            var propertyToUpdate = propertyUpdateDescriptor.ToJavaScriptPropertyExpression();

            return $"\t{propertyToUpdate} = args.{parameterName};";
        }

        private static string CreateConditionVariableScript(string argumentName, string variableName, string conditionPropertyPath)
        {
            return $"var {variableName} = {conditionPropertyPath} < args.{argumentName};";
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