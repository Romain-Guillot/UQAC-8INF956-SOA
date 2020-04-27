using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace StockSDK
{
    public class StockManager
    {
        private readonly IConnection connection;
        private readonly IModel channel;
        private readonly string replyQueueName;
        private readonly EventingBasicConsumer consumer;
        private readonly BlockingCollection<ItemLine> respQueue = new BlockingCollection<ItemLine>();
        private readonly IBasicProperties props;
        
        public StockManager()
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
                try
                {
                    if (ea.BasicProperties.CorrelationId == correlationId)
                    {
                        var body = ea.Body;
                    
                        var itemLine = JsonConvert.DeserializeObject<ItemLine>(Encoding.UTF8.GetString(body));
                        Console.WriteLine("SDK:" + itemLine.Item.Name);
                        respQueue.Add(itemLine);
                    }
                }
                catch (Exception e)
                {
                  Console.WriteLine(e);  
                }
                
            };
        }
        
        public ItemLine ReserveItem(int quantity, string name)
        {
            var messageBytes = Encoding.UTF8.GetBytes(name);
            channel.BasicPublish("",  "stock_queue",  props,  messageBytes);
            channel.BasicConsume(consumer, replyQueueName, true);
            return respQueue.Take();
        }

        public void ReleaseItem(ItemLine itemLine)
        {
            
        }
    }
}