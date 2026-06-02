using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Pantry_To_Plate.mods
{
    public static class PantryService
    {
        private static string path = @"data\Pantry.csv";

        public static List<PantryItem> Load()
        {
            Directory.CreateDirectory("data");
            List<PantryItem> pantry = new List<PantryItem>();

            if (!File.Exists(path))
            {
                return pantry;
            }

            var lines = File.ReadAllLines(path).Skip(1);

            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                string[] parts = line.Split(';');

                if (parts.Length < 6)
                {
                    continue;
                }

                pantry.Add(new PantryItem
                {
                    Food = new FoodItems
                    {
                        Name = parts[0],
                        Calories = ReadDouble(parts, 2),
                        Protein = ReadDouble(parts, 3),
                        Carbs = ReadDouble(parts, 4),
                        Fat = ReadDouble(parts, 5)
                    },
                    AmountInGram = ReadDouble(parts, 1)
                });
            }

            return pantry;
        }

        public static void Save(List<PantryItem> pantry)
        {
            Directory.CreateDirectory("data");

            List<string> lines = new List<string>
            {
                "Name;AmountGram;Calories;Protein;Carbs;Fat"
            };

            foreach (PantryItem item in pantry.OrderBy(i => i.Name))
            {
                if (item.Food == null || string.IsNullOrWhiteSpace(item.Food.Name) || item.AmountInGram <= 0)
                {
                    continue;
                }

                lines.Add(string.Join(";",
                    Clean(item.Food.Name),
                    item.AmountInGram.ToString(CultureInfo.InvariantCulture),
                    item.Food.Calories.ToString(CultureInfo.InvariantCulture),
                    item.Food.Protein.ToString(CultureInfo.InvariantCulture),
                    item.Food.Carbs.ToString(CultureInfo.InvariantCulture),
                    item.Food.Fat.ToString(CultureInfo.InvariantCulture)));
            }

            File.WriteAllLines(path, lines);
        }

        public static void AddOrUpdate(FoodItems food, double amountGram)
        {
            if (food == null || string.IsNullOrWhiteSpace(food.Name) || amountGram <= 0)
            {
                return;
            }

            List<PantryItem> pantry = Load();
            PantryItem existing = pantry.FirstOrDefault(p => string.Equals(p.Name, food.Name, StringComparison.OrdinalIgnoreCase));

            if (existing == null)
            {
                pantry.Add(new PantryItem { Food = food, AmountInGram = amountGram });
            }
            else
            {
                existing.AmountInGram += amountGram;
                existing.Food = food;
            }

            Save(pantry);
            AppLogger.Log($"Pantry aktualisiert: {food.Name}, +{amountGram} g");
        }

        public static List<Ingredient> GetMissingIngredients(Recipe recipe)
        {
            List<PantryItem> pantry = Load();
            List<Ingredient> missing = new List<Ingredient>();

            if (recipe == null || recipe.Ingredients == null)
            {
                return missing;
            }

            foreach (Ingredient needed in CombineSameIngredients(recipe.Ingredients))
            {
                double available = GetAvailableAmount(pantry, needed.FoodName);

                if (available < needed.AmountGram)
                {
                    missing.Add(new Ingredient
                    {
                        FoodName = needed.FoodName,
                        AmountGram = needed.AmountGram - available
                    });
                }
            }

            return missing;
        }

        public static bool HasEnoughIngredients(Recipe recipe)
        {
            return GetMissingIngredients(recipe).Count == 0;
        }

        public static void ConsumeIngredients(Recipe recipe)
        {
            if (recipe == null || recipe.Ingredients == null)
            {
                return;
            }

            List<PantryItem> pantry = Load();

            foreach (Ingredient ingredient in CombineSameIngredients(recipe.Ingredients))
            {
                double remainingToConsume = ingredient.AmountGram;

                foreach (PantryItem item in pantry.Where(p => IsSameOrSimilarFood(p.Name, ingredient.FoodName)))
                {
                    if (remainingToConsume <= 0)
                    {
                        break;
                    }

                    double consumed = Math.Min(item.AmountInGram, remainingToConsume);
                    item.AmountInGram -= consumed;
                    remainingToConsume -= consumed;
                }
            }

            pantry = pantry.Where(p => p.AmountInGram > 0).ToList();
            Save(pantry);

            if (recipe != null)
            {
                AppLogger.Log($"Zutaten für Rezept verbraucht: {recipe.Name}");
            }
        }

        public static double CalculateMatchPercent(Recipe recipe)
        {
            if (recipe == null || recipe.Ingredients == null || recipe.Ingredients.Count == 0)
            {
                return 0;
            }

            List<PantryItem> pantry = Load();
            List<Ingredient> neededIngredients = CombineSameIngredients(recipe.Ingredients);

            if (neededIngredients.Count == 0)
            {
                return 0;
            }

            double percentSum = 0;

            foreach (Ingredient needed in neededIngredients)
            {
                double available = GetAvailableAmount(pantry, needed.FoodName);
                double ingredientPercent = needed.AmountGram <= 0 ? 0 : Math.Min(available / needed.AmountGram, 1.0);
                percentSum += ingredientPercent;
            }

            return percentSum * 100.0 / neededIngredients.Count;
        }

        public static double CalculateMissingGram(Recipe recipe)
        {
            return GetMissingIngredients(recipe).Sum(i => i.AmountGram);
        }

        private static List<Ingredient> CombineSameIngredients(List<Ingredient> ingredients)
        {
            if (ingredients == null)
            {
                return new List<Ingredient>();
            }

            return ingredients
                .Where(i => i != null && !string.IsNullOrWhiteSpace(i.FoodName) && i.AmountGram > 0)
                .GroupBy(i => Normalize(i.FoodName))
                .Select(g => new Ingredient
                {
                    FoodName = g.First().FoodName,
                    AmountGram = g.Sum(i => i.AmountGram)
                })
                .ToList();
        }

        private static double GetAvailableAmount(List<PantryItem> pantry, string foodName)
        {
            return pantry
                .Where(p => p != null && !string.IsNullOrWhiteSpace(p.Name) && IsSameOrSimilarFood(p.Name, foodName))
                .Sum(p => p.AmountInGram);
        }

        private static bool IsSameOrSimilarFood(string pantryName, string ingredientName)
        {
            string pantry = Normalize(pantryName);
            string ingredient = Normalize(ingredientName);

            if (string.IsNullOrWhiteSpace(pantry) || string.IsNullOrWhiteSpace(ingredient))
            {
                return false;
            }

            if (pantry == ingredient)
            {
                return true;
            }

            return pantry.StartsWith(ingredient + " ") ||
                   pantry.StartsWith(ingredient + "-") ||
                   pantry.StartsWith(ingredient + "/") ||
                   ingredient.StartsWith(pantry + " ") ||
                   ingredient.StartsWith(pantry + "-") ||
                   ingredient.StartsWith(pantry + "/");
        }

        private static string Normalize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "";
            }

            string normalized = value.ToLowerInvariant().Trim();
            normalized = normalized.Replace("ä", "ae").Replace("ö", "oe").Replace("ü", "ue").Replace("ß", "ss");
            normalized = Regex.Replace(normalized, @"\s+", " ");
            return normalized;
        }

        private static double ReadDouble(string[] parts, int index)
        {
            if (parts.Length <= index)
            {
                return 0;
            }

            double value;
            if (double.TryParse(parts[index].Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out value))
            {
                return value;
            }

            return 0;
        }

        private static string Clean(string value)
        {
            if (value == null)
            {
                return "";
            }

            return value.Replace(";", ",").Trim();
        }
    }
}