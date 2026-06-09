using Pantry_To_Plate.mods;
using Pantry_To_Plate.windows;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;

namespace Pantry_To_Plate
{
    public partial class MainWindow : Window
    {
        private userinfo user;

        public MainWindow()
        {
            InitializeComponent();
            user = UserDataService.Load();
            UpdateDailyValues();
        }

        private double LoadBurnedCaloriesToday()
        {
            //chatgpt: wie mache ich das es jeden tag resettet wird?
            double burnedCalories = FitnessEntryService.LoadBurnedCaloriesToday();
            //chatgpt ende
            return burnedCalories;
        }

        private void UpdateDailyValues()
        {
            user = UserDataService.Load();
            var entries = DailyEntryService.LoadToday();

            double eatenCalories = entries.Sum(e => e.Calories);
            double eatenProtein = entries.Sum(e => e.Protein);
            double eatenCarbs = entries.Sum(e => e.Carbs);
            double eatenFat = entries.Sum(e => e.Fat);
            double burnedCalories = LoadBurnedCaloriesToday();
            double netCalories = eatenCalories - burnedCalories;
            double remainingCalories = user.Kalorienziel - netCalories;

            KalorienzielText.Content =
                $"Kalorienziel: {user.Kalorienziel:F0} kcal\n" +
                $"Gegessen: {eatenCalories:F0} kcal\n" +
                $"Verbrannt: {burnedCalories:F0} kcal\n" +
                $"Übrig: {remainingCalories:F0} kcal";

            ProteineCounterLabel.Content = $"{eatenProtein:F0} g";
            CarbsCounterLabel.Content = $"{eatenCarbs:F0} g";
            FettCounterLabel.Content = $"{eatenFat:F0} g";

            KalorienProgressBar.UpdateBar(netCalories, user.Kalorienziel);

            if (ListBoxTodayMeals != null)
            {
                ListBoxTodayMeals.ItemsSource = null;
                ListBoxTodayMeals.ItemsSource = entries;
            }

            if (TodaySummaryText != null)
            {
                TodaySummaryText.Text = $"{entries.Count} Mahlzeiten · {eatenCalories:F0} kcal";
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            EinstellungenWindow einstellungenWindow = new EinstellungenWindow(user);
            einstellungenWindow.Owner = this;
            einstellungenWindow.ShowDialog();
            UpdateDailyValues();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            RezepteWindow rezepteWindow = new RezepteWindow();
            rezepteWindow.Owner = this;
            rezepteWindow.ShowDialog();
            UpdateDailyValues();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            MahlzeitHinzufügenWindow mahlzeitHinzufügenWindow = new MahlzeitHinzufügenWindow();
            mahlzeitHinzufügenWindow.Owner = this;
            mahlzeitHinzufügenWindow.ShowDialog();
            UpdateDailyValues();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            PantryWindow pantryWindow = new PantryWindow();
            pantryWindow.Owner = this;
            pantryWindow.ShowDialog();
            UpdateDailyValues();
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            fitnessaktivitäthinzufügenwindow    fitwin = new fitnessaktivitäthinzufügenwindow();
            fitwin.Owner = this;
            fitwin.ShowDialog();
            UpdateDailyValues();
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            PantryWindow pantryWindow = new PantryWindow();
            pantryWindow.Owner = this;
            pantryWindow.ShowDialog();
            UpdateDailyValues();
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            RezepteWindow rezepteWindow = new RezepteWindow();
            rezepteWindow.Owner = this;
            rezepteWindow.ShowDialog();
            UpdateDailyValues();
        }

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            UpdateDailyValues();
        }

        private void ProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
        }

        private void BtnEinkaufliste_Click(object sender, RoutedEventArgs e)
        {

            EinkaufslisteWindow einkauflisteWindow = new EinkaufslisteWindow();
            // Shoutout an David 10^2 und Valentin leider ned 10^2 für eana sine hilfe, valentin ohne di wüsst i imma no ned wia dass ma > in vsc drucka muss zum pdf erstella und david fallt ma etzt nix i aba trotzdem shoutout an di<3
            einkauflisteWindow.Show();

        }
    }
}
