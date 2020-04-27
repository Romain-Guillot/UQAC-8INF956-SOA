using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;


namespace MessagingSDK
{
    public class ServerMessaging
    {

        private IConnection _connection;
        private IModel _channel;
        private string _queueName;
        
        public ServerMessaging(string hostName, string queueName)
        {
            _queueName = queueName;
            var factory = new ConnectionFactory { HostName = hostName };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(_queueName, false, false, false, null);
            _channel.BasicQos(0, 1, false);
        }

        public void Listen(Action<BasicDeliverEventArgs, Dictionary<string, object>> callback)
        {
            var consumer = new EventingBasicConsumer(_channel);
            _channel.BasicConsume(_queueName, false, consumer);
            consumer.Received += (model, ea) =>
            {
                try
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    var json = JsonConvert.DeserializeObject<Dictionary<string, object>>(message);
                    callback(ea, json);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Bad formatting json: {e.Message}");
                    callback(ea, null);
                }
            };
        }

        public void Send(BasicDeliverEventArgs senderEa, Dictionary<string, object> responseBody)
        {
            var props = senderEa.BasicProperties;
            var replyProps = _channel.CreateBasicProperties();
            replyProps.CorrelationId = props.CorrelationId;
            byte[] messagebuffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(responseBody));
            Console.WriteLine(JsonConvert.SerializeObject(responseBody));
            _channel.BasicPublish("", props.ReplyTo, replyProps, messagebuffer);
            _channel.BasicAck(senderEa.DeliveryTag, false);
        }
        

        public void Close()
        {
            _connection.Close();
        }
    }
}