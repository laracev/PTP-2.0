using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Pantry_To_Plate.mods
{
    public static class FoodCatalogService
    {
        private static string path = @"data/test_utf8.csv";

        public static List<FoodItems> LoadAll()
        {
            List<FoodItems> foods = new List<FoodItems>();

            if (!File.Exists(path))
            {
                AppLogger.LogWarning("test_utf8.csv wurde nicht gefunden.");
                return foods;
            }
            var lines = File.ReadAllLines(path, Encoding.Latin1).Skip(1);

            foreach (string line in lines)
            {
                string[] parts = line.Split(';');

                if (parts.Length < 1 || string.IsNullOrWhiteSpace(parts[0]))
                {
                    continue;
                }

                foods.Add(new FoodItems
                {
                    Name = parts[0].Trim().TrimStart('\uFEFF'), // Aus dem internet
                    Calories = ReadDouble(parts, 1),
                    Protein = ReadDouble(parts, 2),
                    Fat = ReadDouble(parts, 3),
                    Carbs = ReadDouble(parts, 4),
                    Ballast = parts.Length > 5 ? parts[5] : ""
                });
            }

            return foods;
        }

        public static FoodItems FindByName(string name)
        {
            return LoadAll().FirstOrDefault(f => string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase)); //diese line hab ich von chatgpt, weil nix funktioniert hat, aber jetzt funktioniert es, danke chatgpt 
        }

        private static double ReadDouble(string[] parts, int index)
        {
            if (parts.Length <= index)
            {
                return 0;
            }

            string text = parts[index].Replace(",", ".").Replace("<LOD", "0").Trim(); //diese line hab ich ausm internet
            double value;
            if (double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
            {
                return value;
            }

            return 0;
        }
    }
}
