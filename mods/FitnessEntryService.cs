using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Pantry_To_Plate.mods
{
    public static class FitnessEntryService
    {
        private static string path = @"data/FitnessEintraege.csv";

        public static List<FitnessEntry> LoadAll()
        {
            Directory.CreateDirectory("data");
            List<FitnessEntry> entries = new List<FitnessEntry>();

            if (!File.Exists(path))
            {
                return entries;
            }

            foreach (string line in File.ReadAllLines(path).Skip(1))
            {
                string[] parts = line.Split(';');

                if (parts.Length < 4)
                {
                    continue;
                }

                DateTime date;
                if (!DateTime.TryParse(parts[0], out date))
                {
                    date = DateTime.Today;
                }

                entries.Add(new FitnessEntry
                {
                    Date = date,
                    ActivityName = parts[1],
                    DurationMinutes = ReadDouble(parts, 2),
                    Calories = ReadDouble(parts, 3)
                });
            }

            return entries;
        }

        public static List<FitnessEntry> LoadToday()
        {
            return LoadAll().Where(e => e.Date.Date == DateTime.Today).ToList();
        }

        public static void Add(FitnessEntry entry)
        {
            Directory.CreateDirectory("data");
            bool fileExists = File.Exists(path);

            using (StreamWriter writer = new StreamWriter(path, true))
            {
                if (!fileExists)
                {
                    writer.WriteLine("Datum;Aktivitaet;DauerMinuten;Kalorien");
                }

                writer.WriteLine(
                    entry.Date.ToString("yyyy-MM-dd") + ";" +
                    Clean(entry.ActivityName) + ";" +
                    entry.DurationMinutes.ToString(CultureInfo.InvariantCulture) + ";" +
                    entry.Calories.ToString(CultureInfo.InvariantCulture));
            }

            AppLogger.Log($"Fitness-Eintrag gespeichert: {entry.ActivityName}, {entry.DurationMinutes} Minuten, {entry.Calories} Kalorien");
        }

        public static void SaveAll(List<FitnessEntry> entries)
        {
            Directory.CreateDirectory("data");
            List<string> lines = new List<string>();
            lines.Add("Datum;Aktivitaet;DauerMinuten;Kalorien");

            foreach (FitnessEntry entry in entries)
            {
                lines.Add(
                    entry.Date.ToString("yyyy-MM-dd") + ";" +
                    Clean(entry.ActivityName) + ";" +
                    entry.DurationMinutes.ToString(CultureInfo.InvariantCulture) + ";" +
                    entry.Calories.ToString(CultureInfo.InvariantCulture));
            }

            File.WriteAllLines(path, lines);
            AppLogger.Log("Fitness-Einträge gespeichert.");
        }

        public static void Delete(FitnessEntry entry)
        {
            if (entry == null)
            {
                return;
            }

            List<FitnessEntry> entries = LoadAll();
            FitnessEntry toRemove = entries.FirstOrDefault(e =>
                e.Date.Date == entry.Date.Date &&
                string.Equals(e.ActivityName, entry.ActivityName, StringComparison.OrdinalIgnoreCase) &&
                Math.Abs(e.DurationMinutes - entry.DurationMinutes) < 0.01 &&
                Math.Abs(e.Calories - entry.Calories) < 0.01);

            if (toRemove != null)
            {
                entries.Remove(toRemove);
                SaveAll(entries);
                AppLogger.Log($"Fitness-Eintrag gelöscht: {entry.ActivityName}");
            }
        }

        public static double LoadBurnedCaloriesToday()
        {
            return LoadToday().Sum(e => e.Calories);
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
