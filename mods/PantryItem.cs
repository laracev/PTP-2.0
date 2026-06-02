using System;
using System.Collections.Generic;
using System.Text;

namespace Pantry_To_Plate.mods
{
    public class PantryItem
    {
        public FoodItems Food { get; set; }
        public double AmountInGram { get; set; }

        public string Name
        {
            get { return Food != null ? Food.Name : ""; }
        }

        public double Calories
        {
            get { return Food != null ? Food.Calories * AmountInGram / 100.0 : 0; }
        }

        public string DisplayText
        {
            get { return $"{Name} - {AmountInGram:F0} g"; }
        }
    }
}
