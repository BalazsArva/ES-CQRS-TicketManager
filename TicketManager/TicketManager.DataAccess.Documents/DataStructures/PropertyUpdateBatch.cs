using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace TicketManager.DataAccess.Documents.DataStructures
{
    public class PropertyUpdateBatch<TDocument>
    {
        private readonly List<PropertyUpdateDescriptor> propertyUpdates = new List<PropertyUpdateDescriptor>();

        public PropertyUpdateBatch()
        {
        }

        public PropertyUpdateBatch<TDocument> Add<TProperty>(Expression<Func<TDocument, TProperty>> memberSelector, TProperty newValue)
        {
            propertyUpdates.Add(new PropertyUpdateDescriptor<TDocument, TProperty>(memberSelector, newValue));

            return this;
        }

        public PropertyUpdateDescriptor[] CreateBatch()
        {
            return propertyUpdates.ToArray();
        }
    }
}