using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Pantry_To_Plate.mods
{
    public static class DailyEntryService
    {
        private static string GetPath()
        {
            Directory.CreateDirectory("data");
            return $@"data\gegessen_{DateTime.Today:yyyy-MM-dd}.csv";
        }

        public static void Add(DailyEntry entry)
        {
            string path = GetPath();
            bool fileExists = File.Exists(path);

            using (StreamWriter writer = new StreamWriter(path, true))
            {
                if (!fileExists)
                {
                    writer.WriteLine("FoodName;AmountGram;Calories;Protein;Carbs;Fat");
                }

                writer.WriteLine(
                    Clean(entry.FoodName) + ";" +
                    entry.AmountGram.ToString(CultureInfo.InvariantCulture) + ";" +
                    entry.Calories.ToString(CultureInfo.InvariantCulture) + ";" +
                    entry.Protein.ToString(CultureInfo.InvariantCulture) + ";" +
                    entry.Carbs.ToString(CultureInfo.InvariantCulture) + ";" +
                    entry.Fat.ToString(CultureInfo.InvariantCulture)
                );
            }

            AppLogger.Log("Neuer Tages-Eintrag gespeichert.");
        }

        public static List<DailyEntry> LoadToday()
        {
            List<DailyEntry> entries = new List<DailyEntry>();
            string path = GetPath();

            if (!File.Exists(path))
            {
                return entries;
            }

            var lines = File.ReadAllLines(path).Skip(1);

            foreach (string line in lines)
            {
                string[] parts = line.Split(';');

                if (parts.Length >= 6)
                {
                    entries.Add(new DailyEntry
                    {
                        FoodName = parts[0],
                        AmountGram = ReadDouble(parts, 1),
                        Calories = ReadDouble(parts, 2),
                        Protein = ReadDouble(parts, 3),
                        Carbs = ReadDouble(parts, 4),
                        Fat = ReadDouble(parts, 5)
                    });
                }
            }

            return entries;
        }

        private static double ReadDouble(string[] parts, int index)
        {
            double value;
            if (parts.Length > index && double.TryParse(parts[index].Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out value))
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
