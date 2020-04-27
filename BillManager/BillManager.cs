using MessagingSDK;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UserSDk;
using BillSDK;
using StockSDK;

namespace BillManager
{
    class BillManager
    {
        private Dictionary<string, Bill> _bills;
        BillManager()
        {
            var serverRabbitMQ = new ServerMessaging("localhost", "bill_queue");
            serverRabbitMQ.Listen((ea, json) => {
                if (json == null)
                    serverRabbitMQ.Send(ea, BuildErrorResponse("Bad request formatting."));
                try
                {

                    User user = (User)json["user"];
                    List<BillLine> billLines = (List<BillLine>)json["billLines"];
                    Dictionary<string, object> response;
                    double totalsanstaxes = 0;
                    double totalTTC = 0;
                    foreach (BillLine line in billLines)
                    {
                        totalsanstaxes += line.Quantity * line.TotalSansTaxe;

                    }
                    totalTTC = totalsanstaxes + (totalsanstaxes * 20) / 100;
                    var bill = new Bill(user, billLines, totalsanstaxes, totalTTC);
                   response = new Dictionary<string, object> { { "bill", JsonConvert.SerializeObject(bill) } };

                   

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
        private Dictionary<string, object> BuildErrorResponse(string message)
        {
            return new Dictionary<string, object> { { "error", message } };
        }

        static void Main(string[] args)
        {
           
            var billManager = new BillManager();
        }
    }
}