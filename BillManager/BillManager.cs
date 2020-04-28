using MessagingSDK;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UserSDk;
using BillSDK;


namespace BillManager
{
    class BillManager
    {
        BillManager()
        {
            var serverRabbitMQ = new ServerMessaging("localhost", "bill_queue");
            serverRabbitMQ.Listen((ea, requestBody) => {
                if (requestBody == null)
                    serverRabbitMQ.Send(ea, ServerMessaging.BuildErrorResponse("Bad request formatting."));
                try
                {
                    var response = BuildBill(requestBody);
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

        public Dictionary<string, object> BuildBill(Dictionary<string, object> request)
        {
            User user = JsonConvert.DeserializeObject<User>((string) request["user"]);
            List<BillLine> billLines = JsonConvert.DeserializeObject<List<BillLine>>((string) request["products"]);
            double totalsanstaxes = 0;
            double totalTTC = 0;
            foreach (var line in billLines)
            {
                totalsanstaxes +=  line.TotalSansTaxe;
            }
            totalTTC = totalsanstaxes + (totalsanstaxes * 20) / 100;
            var bill = new Bill(user, billLines, totalsanstaxes, totalTTC);
            return new Dictionary<string, object> { { "bill", JsonConvert.SerializeObject(bill) } };
        }

        static void Main(string[] args)
        {
            var billManager = new BillManager();
        }
    }
}