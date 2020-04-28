using MessagingSDK;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UserSDk;

namespace UserManager
{
    class UserManager
    {
        private Dictionary<string, User> _user;

        UserManager()
        {
            LoadUser();
            var serverRabbitMQ = new ServerMessaging("localhost", "user_queue");
            serverRabbitMQ.Listen((ea, json) => {
                if (json == null)
                    serverRabbitMQ.Send(ea, BuildErrorResponse("Bad request formatting."));
                try
                {
                    string username = (string)json["Username"];
                    Dictionary<string, object> response;
                    if (_user.ContainsKey(username))
                    {
                        var user = _user[username];
                        response = new Dictionary<string, object>{ { "user", JsonConvert.SerializeObject(user)} };
                    }
                    else
                    {
                        response = BuildErrorResponse("Cannot find user.");
                    }

                    serverRabbitMQ.Send(ea, response);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    serverRabbitMQ.Send(ea, BuildErrorResponse(e.Message));
                }
            });
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
            serverRabbitMQ.Close();
        }

        private void LoadUser()
        {
            _user = new Dictionary<string, User>();
            var users = JsonConvert.DeserializeObject<IEnumerable<User>>(File.ReadAllText("../data/users/users.json"));
            _user = users.ToDictionary(u => u.Username, u => u);
        }
        
        private Dictionary<string, object> BuildErrorResponse(string message)
        {
            return new Dictionary<string, object> { { "error", message } };
        }

        static void Main()
        {
            var userManager = new UserManager();
        }
    }
}