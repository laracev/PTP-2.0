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

        public fitnessaktivitäthinzufügenwindow()
        {
            InitializeComponent();
            LoadCsv();
            ListBoxAktivitaeten.ItemsSource = activities;
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

            Directory.CreateDirectory("data");
            string path = @"data/FitnessEintraege.csv";

            if (!File.Exists(path))
            {
                File.WriteAllText(path, "Datum;Aktivitaet;DauerMinuten;Kalorien\n");
            }

            string line = $"{DateTime.Today:yyyy-MM-dd};{selectedActivity.Name};{dauerMinuten.ToString(CultureInfo.InvariantCulture)};{verbrannteKalorien.ToString(CultureInfo.InvariantCulture)}\n";
            File.AppendAllText(path, line);

            AppLogger.Log($"Fitness-Eintrag gespeichert: {selectedActivity.Name}, {dauerMinuten} Minuten, {verbrannteKalorien} Kalorien");
            MessageBox.Show($"{selectedActivity.Name} wurde gespeichert.\nDauer: {dauerMinuten:F0} Minuten\nVerbrannt: {verbrannteKalorien:F0} kcal");
            Close();
        }

        private void ListBoxAktivitaeten_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }
    }
}
