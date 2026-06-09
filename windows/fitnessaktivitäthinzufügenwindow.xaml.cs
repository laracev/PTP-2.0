using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Pantry_To_Plate.mods;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace Pantry_To_Plate.windows
{
    public partial class fitnessaktivitäthinzufügenwindow : Window
    {
        List<fitnessactivity> activities = new List<fitnessactivity>();
        private fitnessactivity selectedActivity;

        public fitnessaktivitäthinzufügenwindow()
        {
            InitializeComponent();
            LoadCsv();
            ShowActivities("");
            LoadTodayActivities();
            UpdatePreview();
        }

        private void LoadCsv()
        {
            string path = @"data/MET_Werte_Tabelle.csv";

            if (!File.Exists(path))
            {
                MessageBox.Show("Fitnessaktivitäten-Datei wurde nicht gefunden.");
                AppLogger.LogError("Fitnessaktivitäten-Datei wurde nicht gefunden.");
                return;
            }

            var lines = File.ReadAllLines(path, Encoding.Latin1);
            AppLogger.Log("Fitnessaktivitäten-Datei geladen.");

            foreach (string line in lines.Skip(1))
            {
                string[] parts = line.Split(';');

                if (parts.Length >= 2 && !string.IsNullOrWhiteSpace(parts[0]))
                {
                    double met;
                    if (!double.TryParse(parts[1].Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out met))
                    {
                        met = 0;
                    }

                    activities.Add(new fitnessactivity { Name = parts[0], Met = met });
                }
            }

            AppLogger.Log($"CSV geladen. {activities.Count} Aktivitäten geladen.");
        }

        private void ShowActivities(string searchText)
        {
            string input = Normalize(searchText);
            IEnumerable<fitnessactivity> query = activities;

            if (!string.IsNullOrWhiteSpace(input))
            {
                query = activities
                    .Where(a => !string.IsNullOrWhiteSpace(a.Name) && Normalize(a.Name).Contains(input))
                    .OrderBy(a => GetSearchRank(a.Name, input))
                    .ThenBy(a => a.Name.Length)
                    .ThenBy(a => a.Name);
            }
            else
            {
                query = activities.OrderBy(a => a.Name);
            }

            ListBoxAktivitaeten.ItemsSource = null;
            ListBoxAktivitaeten.ItemsSource = query.Take(50).ToList();
        }

        private void LoadTodayActivities()
        {
            ListBoxAktivitaeten1.ItemsSource = null;
            ListBoxAktivitaeten1.ItemsSource = FitnessEntryService.LoadToday();
        }

        private void UpdatePreview()
        {
            if (TxtPreviewCalories == null)
            {
                return;
            }

            double dauerMinuten;
            if (selectedActivity == null || !double.TryParse(TxtBoxDauerMinuten.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out dauerMinuten) || dauerMinuten <= 0)
            {
                TxtPreviewCalories.Text = "Vorschau: 0 kcal";
                return;
            }

            userinfo user = UserDataService.Load();
            double gewichtKg = user.Weight;

            if (gewichtKg <= 0)
            {
                TxtPreviewCalories.Text = "Vorschau: Gewicht fehlt";
                return;
            }

            double calories = selectedActivity.CalculateCalories(gewichtKg, dauerMinuten);
            TxtPreviewCalories.Text = $"Vorschau: {calories:F0} kcal";
        }

        private void Schliessen_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SpeichernUndSchliessen_Click(object sender, RoutedEventArgs e)
        {
            if (ListBoxAktivitaeten.SelectedItem == null)
            {
                MessageBox.Show("Bitte eine Aktivität auswählen.");
                AppLogger.LogWarning("Keine Aktivität ausgewählt.");
                return;
            }

            double dauerMinuten;
            if (!double.TryParse(TxtBoxDauerMinuten.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out dauerMinuten) || dauerMinuten <= 0)
            {
                MessageBox.Show("Bitte eine gültige Dauer eingeben.");
                AppLogger.LogWarning("Keine gültige Dauer angegeben.");
                return;
            }

            fitnessactivity selectedActivity = (fitnessactivity)ListBoxAktivitaeten.SelectedItem;
            userinfo user = UserDataService.Load();
            double gewichtKg = user.Weight;

            if (gewichtKg <= 0)
            {
                MessageBox.Show("Bitte zuerst dein Gewicht in den Einstellungen speichern.");
                AppLogger.LogWarning("Kein gültiges Benutzergewicht vorhanden.");
                return;
            }

            double verbrannteKalorien = selectedActivity.CalculateCalories(gewichtKg, dauerMinuten);

            FitnessEntryService.Add(new FitnessEntry
            {
                Date = DateTime.Today,
                ActivityName = selectedActivity.Name,
                DurationMinutes = dauerMinuten,
                Calories = verbrannteKalorien
            });

            MessageBox.Show($"{selectedActivity.Name} wurde gespeichert.\nDauer: {dauerMinuten:F0} Minuten\nVerbrannt: {verbrannteKalorien:F0} kcal");
            LoadTodayActivities();
            UpdatePreview();
        }

        private void ListBoxAktivitaeten_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedActivity = ListBoxAktivitaeten.SelectedItem as fitnessactivity;
            UpdatePreview();
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ShowActivities(TxtSearch.Text);
        }

        private void TxtBoxDauerMinuten_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdatePreview();
        }

        private void SetDuration_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            if (button != null)
            {
                TxtBoxDauerMinuten.Text = button.Tag.ToString();
            }
        }

        private void DeleteSelectedFitness_Click(object sender, RoutedEventArgs e)
        {
            FitnessEntry selected = ListBoxAktivitaeten1.SelectedItem as FitnessEntry;

            if (selected == null)
            {
                MessageBox.Show("Bitte zuerst eine gespeicherte Aktivität auswählen.");
                return;
            }

            FitnessEntryService.Delete(selected);
            LoadTodayActivities();
        }

        private int GetSearchRank(string activityName, string input)
        {
            string name = Normalize(activityName);

            if (name == input)
            {
                return 0;
            }

            if (name.StartsWith(input))
            {
                return 1;
            }

            if (name.Contains(" " + input) || name.Contains("-" + input) || name.Contains("/" + input))
            {
                return 2;
            }

            return 3;
        }

        private string Normalize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "";
            }

            return value.ToLowerInvariant().Trim().Replace("ä", "ae").Replace("ö", "oe").Replace("ü", "ue").Replace("ß", "ss");
        }
    }
}
