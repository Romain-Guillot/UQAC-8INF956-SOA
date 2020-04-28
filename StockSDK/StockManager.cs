using System;
using System.Collections.Generic;
using MessagingSDK;
using Newtonsoft.Json;


namespace StockSDK
{
    public class StockManager
    {
        private ClientMessaging _clientMessaging;
        
        public StockManager()
        {
            _clientMessaging = new ClientMessaging("stock_queue");
        }

        public List<Item> GetProducts()
        {
            var request = new Dictionary<string, object>
            {
                {"action", "list"}
            };
            var response = _clientMessaging.Send(request);
            return JsonConvert.DeserializeObject<List<Item>>((string) response["items"]);
        }
        
        public ItemLine ReserveItem(int quantity, string name)
        {
            var request = new Dictionary<string, object>
            {
                {"action", "reserve"},
                {"product", name},
                {"quantity", quantity}
            };
            var response = _clientMessaging.Send(request);
            if (!response.ContainsKey("nReserved"))
                throw new Exception(response.ContainsKey("error") ? (string) response["error"] : "Error occured");
            var nReserved = (int)(long) response["nReserved"];
            return new ItemLine(new Item(name, quantity), nReserved);
        }

        public void ReleaseItem(ItemLine itemLine)
        {
            var request = new Dictionary<string, object>
            {
                {"action", "release"},
                {"product", itemLine.Item.Name},
                {"quantity", itemLine.Quantity}
            };
            _clientMessaging.Send(request);
        }

        public void Close()
        {
            _clientMessaging.Close();
        }
    }
}