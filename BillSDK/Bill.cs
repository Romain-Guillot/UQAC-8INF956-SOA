using MessagingSDK;
using Newtonsoft.Json;
using StockSDK;
using System;
using System.Collections.Generic;
using System.Linq;
using UserSDk;


namespace BillSDK
{
    public class Bill
    {
        public User User;
        public IEnumerable<BillLine> BillLines;
        public double TotalHT;
        public double TotalTTC;
        
        public Bill(User user, IEnumerable<BillLine> billLines, double totalsanstaxes, double totalTTC)
        {
            User = user;
            BillLines = billLines;
            TotalHT = totalsanstaxes;
            TotalTTC = totalTTC;
        }
        
        public static Bill CreateBill(User user, IEnumerable<ItemLine> lines)
        {
            ClientMessaging _clientMessaging = new ClientMessaging("bill_queue");
            List<BillLine> billLines = new List<BillLine>();
            foreach (ItemLine line in lines)
            {
                var billline = new BillLine(line.Item, line.Quantity);
                billLines.Add(billline);
            }
            var request = new Dictionary<string, object>
            {
                {"user", JsonConvert.SerializeObject(user)},
                {"products", JsonConvert.SerializeObject(billLines)}
            };
            var response = _clientMessaging.Send(request);
            var bill = JsonConvert.DeserializeObject<Bill>((string)response["bill"]);
            _clientMessaging.Close();
            return bill;

        }
        
        public override string ToString() => $"Facture de {User.Username}: {TotalHT}$\nAjout des taxes (20%) : {TotalTTC}$";

        public void PrintBill()
        {
            Console.WriteLine(this);
            Console.WriteLine("Details:");
            Console.WriteLine(BillLine.ToStringHeader());
            BillLines.ToList().ForEach(Console.WriteLine);
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