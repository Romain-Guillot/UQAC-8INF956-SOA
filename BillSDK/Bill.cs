using MessagingSDK;
using Newtonsoft.Json;
using StockSDK;
using System;
using System.Collections.Generic;
using UserSDk;

namespace BillSDK
{
    public class Bill
    {
        public User User;
        public List<BillLine> BillLines;
        public double TotalSansTaxe;
        public double TotalTTC;
        private static ClientMessaging _clientMessaging = new ClientMessaging("bill_queue");


        public Bill(User user, List<BillLine> billLines, double totalsanstaxes, double totalTTC)
        {
            User = user;
            BillLines = billLines;
            TotalSansTaxe = totalsanstaxes;
            TotalTTC = totalTTC;
        }
        public Bill()
        {

        }
        public static Bill CreateBill(User user, List<ItemLine> lines)
        {
            List<BillLine> billLines = new List<BillLine>();
            foreach (ItemLine line in lines)
            {
                var billline = new BillLine(line.Item, line.Quantity, line.Item.Price * line.Quantity);
                billLines.Add(billline);

            }

            var request = new Dictionary<string, object>
            {
                {"user", user},
                {"billLines", billLines}
            };
            var response = _clientMessaging.Send(request);
            var bill = new Bill();
           
            bill = JsonConvert.DeserializeObject<Bill>((string)response["bill"]);


            return bill;

        }


    }
    public class BillLine
    {
        Item Item;
        public int Quantity;
        public double TotalSansTaxe;

        public BillLine(Item item,int qt, double totalSansTaxe)
        {
            Item = item;
            Quantity = qt;
            TotalSansTaxe = totalSansTaxe;
        }

    }
}