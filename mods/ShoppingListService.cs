using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Pantry_To_Plate.mods
{
    public static class ShoppingListService
    {
        private static string path = @"data/Einkaufsliste.csv";

        public static List<Ingredient> Load()
        {
            Directory.CreateDirectory("data");
            List<Ingredient> items = new List<Ingredient>();

            if (!File.Exists(path))
            {
                return items;
            }

            foreach (string line in File.ReadAllLines(path).Skip(1))
            {
                string[] parts = line.Split(';');

                if (parts.Length < 2)
                {
                    continue;
                }

                double amount;
                if (!double.TryParse(parts[1].Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out amount))
                {
                    amount = 0;
                }

                items.Add(new Ingredient { FoodName = parts[0], AmountGram = amount });
                
            }

            return items;
        }

        public static void Save(List<Ingredient> items)
        {
            Directory.CreateDirectory("data");
            List<string> lines = new List<string>();
            lines.Add("FoodName;AmountGram");

            foreach (Ingredient item in items.OrderBy(i => i.FoodName))
            {
                lines.Add(item.FoodName + ";" + item.AmountGram.ToString(CultureInfo.InvariantCulture));
            }

            File.WriteAllLines(path, lines);
        }

        public static void AddMissingIngredients(List<Ingredient> missingIngredients)
        {
            List<Ingredient> shoppingList = Load();

            foreach (Ingredient missing in missingIngredients)
            {
                Ingredient existing = shoppingList.FirstOrDefault(i => string.Equals(i.FoodName, missing.FoodName, StringComparison.OrdinalIgnoreCase));

                if (existing == null)
                {
                    shoppingList.Add(new Ingredient { FoodName = missing.FoodName, AmountGram = missing.AmountGram });
                }
                else
                {
                    existing.AmountGram += missing.AmountGram;
                }
            }

            Save(shoppingList);
            AppLogger.Log("Fehlende Zutaten zur Einkaufsliste hinzugefügt.");
        }

        public static void Clear()
        {
            Save(new List<Ingredient>());
            AppLogger.Log("Einkaufsliste geleert.");
        }
    }
}
