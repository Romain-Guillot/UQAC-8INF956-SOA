using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StockSDK;

namespace StockManager
{
    class ItemStock
    {
        public Item Item;
        public int Quantity;

        public ItemStock(Item item, int quantity)
        {
            Item = item;
            Quantity = quantity;
        }
    }
    class StockManager
    {

        private Dictionary<string, ItemStock> _stock;

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
                    byte[] messagebuffer;
                    var props = ea.BasicProperties;
                    var replyProps = channel.CreateBasicProperties();
                    var body = ea.Body;
                    replyProps.CorrelationId = props.CorrelationId;
                    
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine("Manager: " + message);
                    var itemLine = ReserveItem(1, message);
                    messagebuffer = Encoding.Default.GetBytes(JsonConvert.SerializeObject(itemLine));
                    channel.BasicPublish("", props.ReplyTo, replyProps, messagebuffer);
                    channel.BasicAck(ea.DeliveryTag, false);
                    
                };

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }

        private void LoadStock()
        {
            _stock = new Dictionary<string, ItemStock>
            {
                {"Shampoo", new ItemStock(new Item("Shampoo", 5.0), 5)},
                {"Toothbrush", new ItemStock(new Item("Toothbrush", 2.5), 3)}
            };
        }


        public ItemLine ReserveItem(int quantity, string name)
        {
            var item = _stock[name];
            if (quantity <= item.Quantity)
            {
                item.Quantity -= quantity;
                return new ItemLine(item.Item, quantity);
            }
            return null;
        }


        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            
            var stockManager = new StockManager();
            
            
           
        }
    }
}