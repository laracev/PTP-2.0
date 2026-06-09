using System;
using System.Collections.Generic;
using System.Text;
namespace Pantry_To_Plate.mods
{
    public class DailyEntry
    {
        public string FoodName { get; set; }
        public double AmountGram { get; set; }
        public double Calories { get; set; }
        public double Protein { get; set; }
        public double Carbs { get; set; }
        public double Fat { get; set; }

        public string DisplayText
        {
            get { return $"{FoodName} - {AmountGram:F0} g - {Calories:F0} kcal"; }
        }
    }
}
