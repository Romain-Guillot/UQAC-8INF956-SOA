using MessagingSDK;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace UserSDk
{
    public class User
    {
        public string FirstName;
        public string LastName;
        public string Email;
        public string Username;
        private static ClientMessaging _clientMessaging;


        public User(string firstName, string lastName, string email, string username)
        {
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Username = username;
        }
        public User()
        {
            _clientMessaging = new ClientMessaging("user_queue");
        }
        public static void GetUser(string username)
        {
            var request = new Dictionary<string, object>
            {
                {"Username", username}
            };
            _clientMessaging.Send(request);

        }
        public void Close()
        {
            _clientMessaging.Close();
        }


    }

    
}