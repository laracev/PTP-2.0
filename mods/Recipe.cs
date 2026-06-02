using System;
using System.Collections.Generic;
using System.Linq;

namespace Pantry_To_Plate.mods
{
    public class Recipe
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Preparation { get; set; }
        public List<Ingredient> Ingredients { get; set; } = new List<Ingredient>();

        public double MatchPercent { get; set; }

        public override string ToString()
        {
            if (MatchPercent > 0)
            {
                return $"{Name} ({MatchPercent:F0}% vorhanden)";
            }

            return Name;
        }

        public DailyEntry ToDailyEntry(List<FoodItems> foods)
        {
            AppLogger.Log($"Rezept wird verarbeitet {Name}");
            DailyEntry entry = new DailyEntry();
            entry.FoodName = Name;
            entry.AmountGram = Ingredients.Sum(i => i.AmountGram);

            foreach (Ingredient ingredient in Ingredients)
            {
                FoodItems food = foods.FirstOrDefault(f => string.Equals(f.Name, ingredient.FoodName, StringComparison.OrdinalIgnoreCase));

                if (food == null)
                {
                    AppLogger.LogWarning("Lebensmittel nicht gefunden");
                    continue;
                }

                double factor = ingredient.AmountGram / 100.0;
                entry.Calories += food.Calories * factor;
                entry.Protein += food.Protein * factor;
                entry.Carbs += food.Carbs * factor;
                entry.Fat += food.Fat * factor;

                AppLogger.Log($"Zutat verarbeitet {ingredient.FoodName}, {ingredient.AmountGram}g");
            }

            return entry;
        }
    }
}
