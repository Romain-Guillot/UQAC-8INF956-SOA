using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using UserSDk;

namespace UserManager
{
    class UserManager
    {
        private IEnumerable<User> _user;
        public IEnumerable<User> User => _user;

        UserManager()
        {
            LoadUser();
        }

        private void LoadUser()
        {
            _user = new List<User>
            {
                new User("Christian","Attia","christ@attia.com","Chriattia"),
                new User("Michel","Attia","michou@attia.com","michou"),
                new User("Keke","Attia","squizi@attia.com","squizi")

            };
        }

        static void Main()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            UserManager um = new UserManager();
            
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "rpc_user", durable: false,
                  exclusive: false, autoDelete: false, arguments: null);
                channel.BasicQos(0, 1, false);
                var consumer = new EventingBasicConsumer(channel);
                channel.BasicConsume(queue: "rpc_user",
                  autoAck: false, consumer: consumer);
                Console.WriteLine(" [x] Awaiting RPC requests");

                consumer.Received += (model, ea) =>
                {
                    string response = null;

                    var body = ea.Body;
                    var props = ea.BasicProperties;
                    var replyProps = channel.CreateBasicProperties();
                    replyProps.CorrelationId = props.CorrelationId;

                    try
                    {
                        var message = Encoding.UTF8.GetString(body);
                       
                        foreach(User user in um._user)
                        {
                            
                            Console.WriteLine(message);
                            Console.WriteLine(user.Username);
                            
                            if (user.Username == message)
                            {
                                
                                response = "First name ; " + user.FirstName + " Last Name " + user.LastName + " email " + user.Email ;
                            }
                        }
                        Console.WriteLine(" [.] fib({0})", message);
                        
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(" [.] " + e.Message);
                        response = "";
                    }
                    finally
                    {
                        var responseBytes = Encoding.UTF8.GetBytes(response);
                        channel.BasicPublish(exchange: "", routingKey: props.ReplyTo,
                          basicProperties: replyProps, body: responseBytes);
                        channel.BasicAck(deliveryTag: ea.DeliveryTag,
                          multiple: false);
                    }
                };

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }
         private static int fib(int n)
    {
        if (n == 0 || n == 1)
        {
            return n;
        }

        return fib(n - 1) + fib(n - 2);
    }
    }
}