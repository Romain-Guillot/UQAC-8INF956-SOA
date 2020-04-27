using System;
using System.Collections.Generic;
using System.Text;
using BillSDK;
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
        private StockManager _stockManager = new StockManager();
        private User _user;
        private Bill _bill;

        private List<ItemLine> _card = new List<ItemLine>();

        ClientConsoleApp()
        {
            Console.WriteLine("Welcome !");
            Authentication(); // blocking
            Shopping(); // blocking
            Checkout(); // blocking
            Close();
        }

        private void Authentication()
        {
            Console.Write("Username:");
            string username = Console.ReadLine();
            _user = User.GetUser(username);
            if (_user == null)
            {
                Console.WriteLine("Unknow user, try again");
                Authentication();
            }
        }

        private void Shopping()
        {
            Console.Clear();
            PrintCard();
            while (true)
            {
                Console.Write("\nCHOICE (product name): ");
                string product = Console.ReadLine();
                Console.Write("QUANTITY (number): ");
                int quantiy = int.Parse(Console.ReadLine());
                var itemLine = _stockManager.ReserveItem(quantiy, product);
                if (itemLine != null)
                    _card.Add(itemLine);
                Console.Clear();
                PrintCard();
                if (itemLine == null)
                    Console.WriteLine("Error occured. Try again.");
                Console.Write("Continue shopping ? (Y/N)");
                if (Console.ReadKey().Key != ConsoleKey.Y)
                    break;
            }
        }

        private void Checkout()
        {
            Console.Clear();
            Console.WriteLine("CHECKOUT");
            var bill = Bill.CreateBill(_user,_card);
            bill.PrintBill();
        }

        private void PrintCard()
        {
            Console.WriteLine($"USER: {_user}");
            Console.WriteLine("CARD:");
            if (_card.Count > 0)
            {
                Console.WriteLine(ItemLine.ToStringHeader());
                _card.ForEach(Console.WriteLine);
            }
            else
            {
                Console.WriteLine("Empty card");
            }
        }

        private void Close()
        {
            _stockManager.Close();
            _user.Close();
            Console.WriteLine("Bye.");
        }
        
        
        
        static void Main(string[] args)
        {
            new ClientConsoleApp();
        }
    }
}