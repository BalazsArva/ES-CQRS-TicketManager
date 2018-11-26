using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TicketManager.Common.Utils;

namespace TicketManager.DataAccess.Documents.DataStructures
{
    public class ScriptParameterDictionary : Dictionary<string, object>
    {
        public void AddParameter(string parameterName, object newValue)
        {
            if (ObjectHelper.IsCollection(newValue))
            {
                Add(parameterName, JArray.FromObject(newValue));
            }
            else if (ObjectHelper.IsPrimitive(newValue) || ObjectHelper.IsDateTimeLike(newValue))
            {
                Add(parameterName, newValue);
            }
            else
            {
                throw new NotSupportedException($"The patch operation could not handle the value of type '{newValue.GetType().FullName}'.");
            }
        }
    }
}