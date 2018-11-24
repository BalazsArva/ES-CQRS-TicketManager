namespace TicketManager.DataAccess.Documents.Extensions
{
    public abstract class PropertyUpdate
    {
        protected PropertyUpdate(object newValue)
        {
            NewValue = newValue;
        }

        public object NewValue { get; }

        public abstract string ToJavaScriptPropertyExpression();
    }
}