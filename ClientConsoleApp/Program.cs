using System;
using StockSDK;


namespace ClientConsoleApp
{
    /**
     * a. Elle demandera le username de l’utilisateur
     * b. Une fois authentifier, elle lui permettra d’ajouter des items du stock dans un panier
     * c. Une fois le shopping finit, l’utilisateur pourra demander la facture de tous ses items.
     * d. Faites tout cela en ligne de commande pour vous simplifier la vie.
     */
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("WELCOME");
            var stockManager = new StockManager();
            stockManager.ReserveItem(2, "shampoo");
            Console.WriteLine("Press any key to exit");
            Console.ReadLine();
        }
    }
}