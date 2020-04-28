using System;
using System.Collections.Generic;
using System.Linq;
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
    internal class ClientConsoleApp
    {
        private User _user;
        private Dictionary<string, ItemLine> _card = new Dictionary<string, ItemLine>();
        private IEnumerable<Item> _catalog;

        private ClientConsoleApp()
        {
            Console.WriteLine("Welcome !");
            Authentication(); // blocking
            Shopping(); // blocking
            Checkout(); // blocking
            Console.WriteLine("Bye.");
        }

        private void Authentication()
        {
            Console.Clear();
            do
            {
                try
                {
                    Console.Write("Username:");
                    string username = Console.ReadLine();
                    _user = User.GetUser(username);  // get the user, throw an exception if an error occured
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            } while (_user == null);
        }

        private void Shopping()
        {
            var stockManager = new StockManager();
            _catalog = stockManager.GetProducts();
            bool isFinished = false;
            while (!isFinished)
            {
                Console.Clear();
                PrintHeader();
                Console.WriteLine("\nB: Buy item     R: Release item    F: Print bill");
                Console.Write("ACTION (B/R/F): ");
                var action = Console.ReadKey().Key;
                switch (action)
                {
                    case ConsoleKey.B:
                        Console.Write("\nCHOICE (product name): ");
                        string product = Console.ReadLine();
                        Console.Write("QUANTITY (number): ");
                        int quantity = int.Parse(Console.ReadLine());
                        try
                        {
                            var itemLine = stockManager.ReserveItem(quantity, product);
                            _card[itemLine.Item.Name] = itemLine;
                            Console.WriteLine($"{itemLine.Item.Name} added.");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                        break;
                    case ConsoleKey.R:
                        Console.Write("\nPRODUCT TO RELEASE (product name): ");
                        string productName = Console.ReadLine();
                        try
                        {
                            stockManager.ReleaseItem(_card[productName]);
                            _card.Remove(productName);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                        break;
                    case ConsoleKey.F:
                        break;
                }
                Console.WriteLine("\nPress any key to continue ...");
                Console.ReadKey();
            }
            stockManager.Close();
        }

        private void Checkout()
        {
            Console.Clear();
            Console.WriteLine("CHECKOUT");
            var bill = Bill.CreateBill(_user,_card.Values);
            bill.PrintBill();
        }

        private void PrintHeader()
        {
            Console.WriteLine($"\nUSER: {_user}");
            Console.WriteLine("\nPRODUCTS:");
            foreach (var item in _catalog)
            {
                Console.WriteLine(item);
            }
            Console.WriteLine("\nCARD:");
            if (_card.Count > 0)
            {
                Console.WriteLine(ItemLine.ToStringHeader());
                _card.Values.ToList().ForEach(Console.WriteLine);
            }
            else
            {
                Console.WriteLine("Empty card");
            }
        }

        static void Main(string[] args)
        {
            new ClientConsoleApp();
        }
    }
}