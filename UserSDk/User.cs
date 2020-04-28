using System;
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
            ClientMessaging clientMessaging = new ClientMessaging("user_queue");
            var request = new Dictionary<string, object>
            {
                {"Username", username}
            };
            var response = clientMessaging.Send(request);

            if (!response.ContainsKey("user"))
            {
                clientMessaging.Close();
                throw new Exception(response.ContainsKey("error") ? (string) response["error"] : "Error occured");
            }
            User user = JsonConvert.DeserializeObject<User>((string) response["user"]);
            clientMessaging.Close();
            return user;
           
        }

        public override string ToString() => $"{Username}: {FirstName} {LastName} - {Email}";
    }
}