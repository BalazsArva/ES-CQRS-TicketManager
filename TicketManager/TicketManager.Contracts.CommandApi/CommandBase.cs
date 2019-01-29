namespace TicketManager.Contracts.CommandApi
{
    public abstract class CommandBase
    {
        protected CommandBase(string raisedByUser)
        {
            RaisedByUser = raisedByUser;
        }

        public string RaisedByUser { get; }
    }
}