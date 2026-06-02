using System;
using System.Collections.Generic;
using System.Text;

namespace Pantry_To_Plate.mods
{
    public class Ingredient
    {
        public string FoodName { get; set; }
        public double AmountGram { get; set; }

        public override string ToString()
        {
            AppLogger.Log($"Ingridient hinzugefügt: {FoodName}, Gewicht: {AmountGram:F0}g");
            return $"{FoodName} - {AmountGram:F0} g";
        }
    }
}
