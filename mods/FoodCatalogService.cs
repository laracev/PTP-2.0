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
        private static readonly string[] PossiblePaths =
        {
            @"data\Lebensmittel.csv",
            @"data\test_utf8.csv"
        };

        public static List<FoodItems> LoadAll()
        {
            List<FoodItems> foods = new List<FoodItems>();
            string path = GetExistingPath();

            if (path == null)
            {
                AppLogger.LogWarning("Keine Lebensmittel-Datei gefunden. Erwartet: data/Lebensmittel.csv oder data/test_utf8.csv");
                return foods;
            }

            var lines = File.ReadAllLines(path, Encoding.Latin1).Skip(1);

            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                string[] parts = line.Split(';');

                if (parts.Length < 2 || string.IsNullOrWhiteSpace(parts[0]))
                {
                    continue;
                }

                foods.Add(ParseFood(parts));
            }

            return foods
                .Where(f => !string.IsNullOrWhiteSpace(f.Name))
                .GroupBy(f => NormalizeName(f.Name))
                .Select(g => g.First())
                .OrderBy(f => f.Name)
                .ToList();
        }

        public static FoodItems FindByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            return LoadAll().FirstOrDefault(f => string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        private static FoodItems ParseFood(string[] parts)
        {
            string name = CleanText(parts[0]);

            // Variante A: Name;Ballast;Calories;Protein;Carbs;Fat
            // Variante B: Name;Calories;Protein;Fat;Carbs;Ballast
            bool secondColumnLooksNumeric = IsNumber(parts, 1);

            if (secondColumnLooksNumeric)
            {
                return new FoodItems
                {
                    Name = name,
                    Calories = ReadDouble(parts, 1),
                    Protein = ReadDouble(parts, 2),
                    Fat = ReadDouble(parts, 3),
                    Carbs = ReadDouble(parts, 4),
                    Ballast = parts.Length > 5 ? CleanText(parts[5]) : ""
                };
            }

            return new FoodItems
            {
                Name = name,
                Ballast = parts.Length > 1 ? CleanText(parts[1]) : "",
                Calories = ReadDouble(parts, 2),
                Protein = ReadDouble(parts, 3),
                Carbs = ReadDouble(parts, 4),
                Fat = ReadDouble(parts, 5)
            };
        }

        private static string GetExistingPath()
        {
            foreach (string path in PossiblePaths)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }

            return null;
        }

        private static bool IsNumber(string[] parts, int index)
        {
            if (parts.Length <= index)
            {
                return false;
            }

            double value;
            return double.TryParse(CleanNumber(parts[index]), NumberStyles.Any, CultureInfo.InvariantCulture, out value);
        }

        private static double ReadDouble(string[] parts, int index)
        {
            if (parts.Length <= index)
            {
                return 0;
            }

            double value;
            if (double.TryParse(CleanNumber(parts[index]), NumberStyles.Any, CultureInfo.InvariantCulture, out value))
            {
                return value;
            }

            return 0;
        }

        private static string CleanNumber(string value)
        {
            if (value == null)
            {
                return "0";
            }

            return value
                .Replace(",", ".")
                .Replace("<LOD", "0")
                .Replace("kcal", "")
                .Replace("g", "")
                .Trim();
        }

        private static string CleanText(string value)
        {
            if (value == null)
            {
                return "";
            }

            return value.Trim().TrimStart('\uFEFF');
        }

        private static string NormalizeName(string value)
        {
            return CleanText(value).ToLowerInvariant();
        }
    }
}