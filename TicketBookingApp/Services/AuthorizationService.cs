using System.Collections.Generic;

namespace TicketBookingApp.Services
{
    public class AuthorizationService
    {
        // Whitelist of authorized users with email and password
        private static readonly Dictionary<string, string> AuthorizedUsers = new Dictionary<string, string>
        {
            { "imrohitchaudhari55@gmail.com", "4Pro@2024Secure" },
            { "rakhpasaremangesh33@gmail.com", "4Pro@2024Secure" }
        };

        public static bool IsAuthorized(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                return false;

            if (AuthorizedUsers.TryGetValue(email.ToLower(), out var storedPassword))
            {
                return password == storedPassword;
            }

            return false;
        }

        public static void UpdateAuthorizedUsers(Dictionary<string, string> users)
        {
            AuthorizedUsers.Clear();
            foreach (var user in users)
            {
                AuthorizedUsers[user.Key.ToLower()] = user.Value;
            }
        }
    }
}
