using System;

namespace Pantry_To_Plate.mods
{
    public class FitnessEntry
    {
        public DateTime Date { get; set; }
        public string ActivityName { get; set; }
        public double DurationMinutes { get; set; }
        public double Calories { get; set; }

        public string DisplayText
        {
            get { return $"{ActivityName} - {DurationMinutes:F0} min - {Calories:F0} kcal"; }
        }
    }
}
