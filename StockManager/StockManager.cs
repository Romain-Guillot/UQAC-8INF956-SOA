using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StockSDK;

namespace StockManager
{
    class StockManager
    {

        private Dictionary<string, Item> _items;
        private Dictionary<Item, int> _stock;
        public IEnumerable<Item> Stock => _items.Values;

        StockManager()
        {
            LoadStock();
            
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare("stock_queue", false, false, false, null);
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
                    var itemLine = ReserveItem(1, message);
                    Console.WriteLine($"Receive : {message}");
                    IFormatter formatter = new BinaryFormatter();  
                    Stream stream = new MemoryStream();
                    formatter.Serialize(stream, itemLine);
                    var responseBytes = Encoding.UTF8.GetBytes(message);
                    channel.BasicPublish( "", props.ReplyTo, replyProps, responseBytes);
                    channel.BasicAck(ea.DeliveryTag, false);
                    
                };

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }

        private void LoadStock()
        {
            _items = new Dictionary<string, Item>
            {
                {"Sampoo", new Item("Shampoo", 5.0)},
                {"Toothbrush", new Item("Toothbrush", 2.5)}
            };
        }


        public ItemLine ReserveItem(int quantity, string name)
        {
            var item = _items[name];
            var stockSize = _stock[item];
            return new ItemLine(item, quantity);
        }


        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            
            var stockManager = new StockManager();
            
            
           
        }
    }
}