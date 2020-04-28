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
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly BlockingCollection<Dictionary<string, object>> _respQueue = new BlockingCollection<Dictionary<string, object>>();
        private readonly IBasicProperties _props;
        private readonly string _queueName;
        
        public ClientMessaging(string queueName)
        {
            this._queueName = queueName;
            var factory = new ConnectionFactory() { HostName = "localhost" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            var replyQueueName = _channel.QueueDeclare().QueueName;
            var consumer = new EventingBasicConsumer(_channel);
            _props = _channel.CreateBasicProperties();
            string correlationId= Guid.NewGuid().ToString();
            _props.CorrelationId = correlationId;
            _props.ReplyTo = replyQueueName;
            _channel.BasicConsume(consumer, replyQueueName, true);

            consumer.Received += (model, ea) =>
            {
                try
                {
                    if (ea.BasicProperties.CorrelationId == correlationId)
                    {
                        var body = ea.Body;
                        var response = JsonConvert.DeserializeObject<Dictionary<string, object>>(Encoding.UTF8.GetString(body));
                        _respQueue.Add(response);
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
            _channel.BasicPublish("",  _queueName,  _props,  messageBytes);
            return _respQueue.Take();
        }
        
        public void Close()
        {
            _connection.Close();
        }
    }
}