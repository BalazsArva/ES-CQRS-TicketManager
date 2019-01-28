using System;
using System.Security.Cryptography;
using System.Text;

namespace TicketManager.DataAccess.Documents.Utilities
{
    public class ETagProvider : IETagProvider
    {
        public string CreateCombinedETagFromDocumentETags(params string[] eTags)
        {
            if (eTags == null)
            {
                throw new ArgumentNullException(nameof(eTags));
            }

            if (eTags.Length == 0)
            {
                throw new ArgumentException("At least 1 e-tag must be provided.", nameof(eTags));
            }

            var joinedEtags = string.Join(".", eTags);
            using (var sha = SHA256.Create())
            {
                var hashBytes = sha.ComputeHash(Encoding.Default.GetBytes(joinedEtags));

                return Convert.ToBase64String(hashBytes);
            }
        }
    }
}