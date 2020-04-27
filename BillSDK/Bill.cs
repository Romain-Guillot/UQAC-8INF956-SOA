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
        
        public static Bill CreateBill(User user, List<ItemLine> lines)
        {
            List<BillLine> billLines = new List<BillLine>();
            foreach (ItemLine line in lines)
            {
                var billline = new BillLine(line.Item, line.Quantity);
                billLines.Add(billline);
            }
            var request = new Dictionary<string, object>
            {
                {"user", JsonConvert.SerializeObject(user)},
                {"billLines", JsonConvert.SerializeObject(billLines)}
            };
            var response = _clientMessaging.Send(request);
            var bill = JsonConvert.DeserializeObject<Bill>((string)response["bill"]);
            return bill;

        }
        
        public override string ToString() => $"Facture de {User.Username}: {TotalSansTaxe}$\nAjout des taxes (20%) : {TotalTTC}$";

        public void PrintBill()
        {
            Console.WriteLine(this);
            Console.WriteLine("Details:");
            Console.WriteLine(BillLine.ToStringHeader());
            BillLines.ForEach(Console.WriteLine);
        }
    }
    
    
    public class BillLine
    {
        public Item Item;
        public int Quantity;
        public double TotalSansTaxe;

        public BillLine(Item item,int qt)
        {
            Item = item;
            Quantity = qt;
            TotalSansTaxe = item.Price * qt;
        }
        public static string ToStringHeader() => "Name\t\tQt\tPrice Alone\tTotal Price";

        public override string ToString() => $"{Item.Name}\t\t{Quantity}\t{Item.Price}$\t\t{TotalSansTaxe}$";
    }
}