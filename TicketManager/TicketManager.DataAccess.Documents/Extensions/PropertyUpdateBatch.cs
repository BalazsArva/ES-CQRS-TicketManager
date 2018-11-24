using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace TicketManager.DataAccess.Documents.Extensions
{
    public class PropertyUpdateBatch<TDocument>
    {
        private readonly List<PropertyUpdate> propertyUpdates = new List<PropertyUpdate>();

        public PropertyUpdateBatch()
        {
        }

        public PropertyUpdateBatch<TDocument> Add<TProperty>(Expression<Func<TDocument, TProperty>> memberSelector, TProperty newValue)
        {
            propertyUpdates.Add(new PropertyUpdate<TDocument, TProperty>(memberSelector, newValue));

            return this;
        }

        public PropertyUpdate[] CreateBatch()
        {
            return propertyUpdates.ToArray();
        }
    }
}