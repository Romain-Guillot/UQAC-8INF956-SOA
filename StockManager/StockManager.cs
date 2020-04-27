using System;
using System.Collections.Generic;
using System.IO;
using MessagingSDK;
using Newtonsoft.Json;
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
            var serverRabbitMQ = new ServerMessaging("localhost", "stock_queue");
            serverRabbitMQ.Listen((ea, json) => {
                if (json == null)
                    serverRabbitMQ.Send(ea, BuildErrorResponse("Bad request formatting."));
                try
                {
                    string action = (string) json["action"];
                    string productName = (string) json["product"];
                    int quantity = (int)(long) json["quantity"];
                    Dictionary<string, object> response;
                    switch (action)
                    {
                        case "reserve":
                            response = ReserveItem(quantity, productName);
                            break;
                        case "release":
                            response = ReleaseItem(quantity, productName);
                            break;
                        default:
                            throw new Exception("Unhandled action.");
                    }
                    serverRabbitMQ.Send(ea, response);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    serverRabbitMQ.Send(ea, BuildErrorResponse(e.Message));
                }
            });
            Console.WriteLine(" Press any key to exit.");
            Console.ReadKey();
            serverRabbitMQ.Close();
        }

        
        private void LoadStock()
        {
            _stock = new Dictionary<string, ItemStock>();
            var stockJson = JsonConvert.DeserializeObject<IEnumerable<Dictionary<string, object>>>(File.ReadAllText("../data/stock/stock.json"));
            foreach (var stockItem in stockJson)
            {
                string productName = (string) stockItem["name"];
                double price = (double) stockItem["price"];
                int quantity = (int)(long) stockItem["qt"];
                _stock[productName] = new ItemStock(new Item(productName, price), quantity);
            }
        }


        public Dictionary<string, object> ReserveItem(int quantity, string productName)
        {
            if (_stock.ContainsKey(productName))
            {
                var item = _stock[productName];
                if (quantity <= item.Quantity)
                {
                    item.Quantity -= quantity;
                    return new Dictionary<string, object>{{"nReserved", quantity}};
                }
            }
            return new Dictionary<string, object>{{"nReserved", 0}};
        }


        public Dictionary<string, object> ReleaseItem(int quantity, string productName)
        {
            if (_stock.ContainsKey(productName))
            {
                _stock[productName].Quantity += quantity;
            }
            return null;
        }
        
        private Dictionary<string, object> BuildErrorResponse(string message)
        {
            return new Dictionary<string, object> {{"error", message}};
        }


        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var stockManager = new StockManager();
        }
    }
}