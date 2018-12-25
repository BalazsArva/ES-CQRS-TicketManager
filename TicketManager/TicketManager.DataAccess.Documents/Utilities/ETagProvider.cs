using System;
using System.Text;

namespace TicketManager.DataAccess.Documents.Utilities
{
    public static class ETagProvider
    {
        public static string CreateETagFromChangeVector(string changeVector)
        {
            if (changeVector == null)
            {
                throw new ArgumentNullException(nameof(changeVector));
            }

            if (changeVector == string.Empty)
            {
                throw new ArgumentException($"The parameter {nameof(changeVector)} cannot be an empty string.", nameof(changeVector));
            }

            // Change vectors in RavenDb are similar to A:1234...
            // The only purpose of this method is to hide the underlying technical stuff and expose
            // etags in a more convenient base64-format.
            return Convert.ToBase64String(Encoding.Default.GetBytes(changeVector));
        }
    }
}