using System.Collections.Generic;

namespace TicketBookingApp.Services
{
    public class AuthorizationService
    {
        // Whitelist of authorized email addresses
        private static readonly HashSet<string> AuthorizedEmails = new HashSet<string>
        {
            "imrohitchaudhari55@gmail.com",
            "rakhpasaremangesh33@gmail.com"
        };

        public static bool IsAuthorized(string email)
        {
            return !string.IsNullOrEmpty(email) && AuthorizedEmails.Contains(email.ToLower());
        }

        public static void UpdateAuthorizedEmails(params string[] emails)
        {
            AuthorizedEmails.Clear();
            foreach (var email in emails)
            {
                if (!string.IsNullOrEmpty(email))
                    AuthorizedEmails.Add(email.ToLower());
            }
        }
    }
}
