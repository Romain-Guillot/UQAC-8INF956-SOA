using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;


namespace MessagingSDK
{
    public class ClientMessaging
    {
        private readonly IConnection connection;
        private readonly IModel channel;
        private readonly string replyQueueName;
        private readonly EventingBasicConsumer consumer;
        private readonly BlockingCollection<Dictionary<string, object>> respQueue = new BlockingCollection<Dictionary<string, object>>();
        private readonly IBasicProperties props;
        private readonly string queueName;
        
        public ClientMessaging(string queueName)
        {
            this.queueName = queueName;
            var factory = new ConnectionFactory() { HostName = "localhost" };
            connection = factory.CreateConnection();
            channel = connection.CreateModel();
            replyQueueName = channel.QueueDeclare().QueueName;
            consumer = new EventingBasicConsumer(channel);
            props = channel.CreateBasicProperties();
            string correlationId= Guid.NewGuid().ToString();
            props.CorrelationId = correlationId;
            props.ReplyTo = replyQueueName;
            channel.BasicConsume(consumer, replyQueueName, true);

            consumer.Received += (model, ea) =>
            {
                try
                {
                    if (ea.BasicProperties.CorrelationId == correlationId)
                    {
                        var body = ea.Body;
                        var response = JsonConvert.DeserializeObject<Dictionary<string, object>>(Encoding.UTF8.GetString(body));
                        respQueue.Add(response);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error" + e);  
                }
            };
        }

        /// Send a request object and return the response
        /// (block until the response arrived)
        public Dictionary<string, object> Send(Dictionary<string, object> request)
        {
            var messageBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request));
            channel.BasicPublish("",  queueName,  props,  messageBytes);
            return respQueue.Take();
        }
        
        public void Close()
        {
            connection.Close();
        }
    }
}