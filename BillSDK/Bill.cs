using MessagingSDK;
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
        public static Bill CreateBill(User user, List<BillLine> billLines)
        {
            double totalsanstaxes = 0;
            var request = new Dictionary<string, object>
            {
                {"user", user},
                {"billLines", billLines}
            };
            var response = _clientMessaging.Send(request);
            

            double totalTTC = totalsanstaxes + (totalsanstaxes * 20)/100;
            return new Bill(user, billLines, totalsanstaxes, totalTTC);

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