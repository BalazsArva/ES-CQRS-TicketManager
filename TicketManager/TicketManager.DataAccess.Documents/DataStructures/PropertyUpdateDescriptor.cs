namespace TicketManager.DataAccess.Documents.DataStructures
{
    public abstract class PropertyUpdateDescriptor
    {
        protected PropertyUpdateDescriptor(object newValue)
        {
            NewValue = newValue;
        }

        public object NewValue { get; }

        public abstract string ToJavaScriptPropertyExpression();
    }
}