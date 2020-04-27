using System;
using System.ComponentModel;

namespace StockSDK
{
    [Serializable]
    public class Item
    {
        public string Name;
        public double Price;

        public Item(string name, double price)
        {
            Name = name;
            Price = price;
        }
    }
}