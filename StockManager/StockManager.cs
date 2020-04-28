using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MessagingSDK;
using Newtonsoft.Json;
using StockSDK;


namespace StockManager
{
    class StockManager
    {
        private Dictionary<string, ItemStock> _stock;

        StockManager()
        {
            LoadStock();
            var serverRabbitMQ = new ServerMessaging("localhost", "stock_queue");
            serverRabbitMQ.Listen((ea, json) => {
                if (json == null)
                    serverRabbitMQ.Send(ea, ServerMessaging.BuildErrorResponse("Bad request formatting."));
                try
                {
                    Dictionary<string, object> response;
                    string action = (string) json["action"];
                    if (action == "list")
                        response = ListItems();
                    else
                    {
                        string productName = (string) json["product"];
                        int quantity = (int)(long) json["quantity"];
                        if (action == "reserve")
                            response = ReserveItem(quantity, productName);
                        else if (action == "release")
                            response = ReleaseItem(quantity, productName);
                        else
                            throw new Exception("Unhandled action.");
                    }
                    serverRabbitMQ.Send(ea, response);
                }
                catch (Exception e)
                {
                    serverRabbitMQ.Send(ea, ServerMessaging.BuildErrorResponse(e.Message));
                }
            });
            Console.WriteLine("Press any key to exit.");
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


        private Dictionary<string, object> ReserveItem(int quantity, string productName)
        {
            if (_stock.ContainsKey(productName))
            {
                var item = _stock[productName];
                if (quantity <= item.Quantity)
                {
                    item.Quantity -= quantity;
                    return new Dictionary<string, object>{{"nReserved", quantity}};
                }
                return ServerMessaging.BuildErrorResponse("Not enough quantity in stock !");
            }
            return ServerMessaging.BuildErrorResponse("Item doesn't exist !");
        }


        private Dictionary<string, object> ReleaseItem(int quantity, string productName)
        {
            if (_stock.ContainsKey(productName))
            {
                _stock[productName].Quantity += quantity;
                return new Dictionary<string, object>{{"nReleased", quantity}};
            }
            return ServerMessaging.BuildErrorResponse("Item doesn't exist !");
        }

        private Dictionary<string, object> ListItems()
        {
            return new Dictionary<string, object> {{"items",JsonConvert.SerializeObject(_stock.Values.Select(stock => stock.Item))}};
        }


        static void Main(string[] args)
        {
            var stockManager = new StockManager();
        }
    }
    
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
}