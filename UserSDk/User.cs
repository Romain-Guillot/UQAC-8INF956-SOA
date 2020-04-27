using MessagingSDK;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace UserSDk
{
    public class User
    {
        public string FirstName;
        public string LastName;
        public string Email;
        public string Username;
        private static ClientMessaging _clientMessaging = new ClientMessaging("user_queue");


        public User(string firstName, string lastName, string email, string username)
        {
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Username = username;
        }
        
        private User() { }
        
        public static User GetUser(string username)
        {
            var request = new Dictionary<string, object>
            {
                {"Username", username}
            };
            var response = _clientMessaging.Send(request);
            var user = JsonConvert.DeserializeObject<User>((string) response["user"]);
            return user;
        }
        
        public void Close()
        {
            _clientMessaging.Close();
        }

        public override string ToString() => $"{Username}: {FirstName} {LastName} - {Email}";
    }
}