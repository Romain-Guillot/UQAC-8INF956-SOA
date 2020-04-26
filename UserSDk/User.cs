using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Text;

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
        
    }

    public class RpcClient
    {
        private readonly IConnection connection;
        private readonly IModel channel;
        private readonly string replyQueueName;
        private readonly EventingBasicConsumer consumer;
        private readonly BlockingCollection<string> respQueue = new BlockingCollection<string>();
        private readonly IBasicProperties props;
        public RpcClient()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };

            connection = factory.CreateConnection();
            channel = connection.CreateModel();
            replyQueueName = channel.QueueDeclare().QueueName;
            consumer = new EventingBasicConsumer(channel);

            props = channel.CreateBasicProperties();
            var correlationId = Guid.NewGuid().ToString();
            props.CorrelationId = correlationId;
            props.ReplyTo = replyQueueName;

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var response = Encoding.UTF8.GetString(body);
                if (ea.BasicProperties.CorrelationId == correlationId)
                {
                    respQueue.Add(response);
                }
            };
        }
        public string getUser(string message)
        {
            var messageBytes = Encoding.UTF8.GetBytes(message);
            channel.BasicPublish(
                exchange: "",
                routingKey: "rpc_user",
                basicProperties: props,
                body: messageBytes);

            channel.BasicConsume(
                consumer: consumer,
                queue: replyQueueName,
                autoAck: true);

            return respQueue.Take();
        }

        public void Close()
        {
            connection.Close();
        }
    }
    
}