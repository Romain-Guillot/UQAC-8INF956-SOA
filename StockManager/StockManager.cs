using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StockSDK;

namespace StockManager
{
    class StockManager
    {

        private IEnumerable<Item> _stock;
        public IEnumerable<Item> Stock => _stock;

        StockManager()
        {
            LoadStock();
        }

        private void LoadStock()
        {
            _stock = new List<Item>
            {
                new Item("Shampoo", 5.0),
                new Item("Toothbrush", 2.5)
            };
        }
        
        
        
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            
            var stockManager = new StockManager();
            
            
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare("rpc_queue", false, false, false, null);
                channel.BasicQos(0, 1, false);
                var consumer = new EventingBasicConsumer(channel);
                channel.BasicConsume("stock_queue", false, consumer);

                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var props = ea.BasicProperties;
                    var replyProps = channel.CreateBasicProperties();
                    replyProps.CorrelationId = props.CorrelationId;
                    var message = Encoding.UTF8.GetString(body);
                    var responseBytes = Encoding.UTF8.GetBytes(message);
                    channel.BasicPublish( "", props.ReplyTo, replyProps, responseBytes);
                    channel.BasicAck(ea.DeliveryTag, false);
                    
                };

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }
    }
}