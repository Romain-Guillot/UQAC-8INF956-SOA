using System;
using System.Collections.Generic;
using System.Text;
using StockSDK;
using UserSDk;

namespace ClientConsoleApp
{
    /**
     * a. Elle demandera le username de l’utilisateur
     * b. Une fois authentifier, elle lui permettra d’ajouter des items du stock dans un panier
     * c. Une fois le shopping finit, l’utilisateur pourra demander la facture de tous ses items.
     * d. Faites tout cela en ligne de commande pour vous simplifier la vie.
     */
    class ClientConsoleApp
    {

        private List<ItemLine> _card = new List<ItemLine>();

        ClientConsoleApp()
        {
            Console.WriteLine("Welcome !");
            Authentication(); // blocking
            Shopping(); // blocking
            Checkout(); // blocking
            Console.WriteLine("Bye.");
        }

        private void Authentication()
        {
            // var rpcClient = new RpcClient();
            // Console.Write("Username:");
            // var username = Console.ReadLine();
            // var response = rpcClient.getUser(username);
            // rpcClient.Close();
        }

        private void Shopping()
        {
            
            Console.Clear();
            PrintCard();
            while (true)
            {

                Console.Write("\nCHOICE: ");
                string product = Console.ReadLine();
                Console.Write("QUANTITY: ");
                int quantiy = int.Parse(Console.ReadLine());
                var stockManager = new StockManager();
                var itemLine = stockManager.ReserveItem(quantiy, product);
                _card.Add(itemLine);
                Console.Clear();
                PrintCard();
                Console.Write("Continue shopping ? (Y/N)");
                if (Console.ReadKey().Key != ConsoleKey.Y)
                    break;
            }
        }

        private void Checkout()
        {
            Console.WriteLine("CHECKOUT");
        }

        private void PrintCard()
        {
            Console.Write(new string('=', 5));
            Console.Write(" CARD ");
            Console.WriteLine(new string('=', 69));
            if (_card.Count > 0)
            {
                Console.WriteLine(ItemLine.ToStringHeader());
                _card.ForEach(Console.WriteLine);
            }
            else
            {
                Console.WriteLine("Empty card");
            }
            Console.WriteLine(new string('=', 80));
        }
        
        
        
        static void Main(string[] args)
        {
            new ClientConsoleApp();
        }
    }
}