using System.Collections.Generic;
using MessagingSDK;


namespace StockSDK
{
    public class StockManager
    {

        private ClientMessaging _clientMessaging;
        
        public StockManager()
        {
            _clientMessaging = new ClientMessaging();
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
            var nReserved = (int)(long) response["nReserved"];
            return nReserved > 0 ? new ItemLine(new Item(name, quantity), nReserved) : null;
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