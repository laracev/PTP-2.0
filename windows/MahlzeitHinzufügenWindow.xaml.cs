using Pantry_To_Plate.mods;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Pantry_To_Plate.windows
{
    public partial class MahlzeitHinzufügenWindow : Window
    {
        private List<FoodItems> foods = new List<FoodItems>();
        private FoodItems selectedFood;
        private List<DailyEntry> todayEntries = new List<DailyEntry>();

        public MahlzeitHinzufügenWindow()
        {
            InitializeComponent();
            LoadFoods();
            LoadTodayEntries();
        }

        private void LoadTodayEntries()
        {
            todayEntries = DailyEntryService.LoadToday();
            ListBoxLebensmittel.ItemsSource = null;
            ListBoxLebensmittel.ItemsSource = todayEntries;
        }

        private void LoadFoods()
        {
            foods = FoodCatalogService.LoadAll();

            if (foods.Count == 0)
            {
                MessageBox.Show("Keine Lebensmittel gefunden. Prüfe bitte data/Lebensmittel.csv oder data/test_utf8.csv.");
            }

            Btn_LebensMittelHinzufuegen.Content = "Lebensmittel hinzufügen";
            Btn_LebensMittelHinzufuegen.IsEnabled = false;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string input = Normalize(TxtBoxLebensmittelHinzufügen.Text);

            ListBoxSuchergebnisse.Items.Clear();
            selectedFood = null;
            Btn_LebensMittelHinzufuegen.Content = "Lebensmittel hinzufügen";
            Btn_LebensMittelHinzufuegen.IsEnabled = false;

            if (string.IsNullOrWhiteSpace(input))
            {
                return;
            }

            var results = foods
                .Where(f => !string.IsNullOrWhiteSpace(f.Name) && Normalize(f.Name).Contains(input))
                .OrderBy(f => GetSearchRank(f.Name, input))
                .ThenBy(f => f.Name.Length)
                .ThenBy(f => f.Name)
                .Take(10)
                .ToList();

            foreach (FoodItems food in results)
            {
                ListBoxSuchergebnisse.Items.Add(food.Name);
            }

            if (results.Count == 0)
            {
                Btn_LebensMittelHinzufuegen.Content = "Kein Treffer";
            }
        }

        private void Btn_LebensMittelHinzufuegen_Click(object sender, RoutedEventArgs e)
        {
            if (selectedFood == null)
            {
                MessageBox.Show("Bitte zuerst ein Lebensmittel aus der Trefferliste auswählen.");
                return;
            }

            double amountGram;
            if (!double.TryParse(TxtBoxMenge.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out amountGram) || amountGram <= 0)
            {
                MessageBox.Show("Bitte eine gültige Menge in Gramm eingeben.");
                return;
            }

            double factor = amountGram / 100.0;

            DailyEntry entry = new DailyEntry
            {
                FoodName = selectedFood.Name,
                AmountGram = amountGram,
                Calories = selectedFood.Calories * factor,
                Protein = selectedFood.Protein * factor,
                Carbs = selectedFood.Carbs * factor,
                Fat = selectedFood.Fat * factor
            };

            DailyEntryService.Add(entry);

            LoadTodayEntries();

            TxtBoxLebensmittelHinzufügen.Clear();
            TxtBoxMenge.Clear();
            ListBoxSuchergebnisse.Items.Clear();

            selectedFood = null;
            Btn_LebensMittelHinzufuegen.Content = "Lebensmittel hinzufügen";
            Btn_LebensMittelHinzufuegen.IsEnabled = false;
        }

        private void ListBoxSuchergebnisse_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListBoxSuchergebnisse.SelectedItem == null)
            {
                selectedFood = null;
                Btn_LebensMittelHinzufuegen.IsEnabled = false;
                return;
            }

            string foodName = ListBoxSuchergebnisse.SelectedItem.ToString();
            selectedFood = foods.FirstOrDefault(f => string.Equals(f.Name, foodName, StringComparison.OrdinalIgnoreCase));

            if (selectedFood != null)
            {
                Btn_LebensMittelHinzufuegen.Content = selectedFood.Name + " hinzufügen";
                Btn_LebensMittelHinzufuegen.IsEnabled = true;
            }
        }

        private void DeleteSelectedMeal_Click(object sender, RoutedEventArgs e)
        {
            DailyEntry selected = ListBoxLebensmittel.SelectedItem as DailyEntry;

            if (selected == null)
            {
                MessageBox.Show("Bitte zuerst eine Mahlzeit aus der Liste auswählen.");
                return;
            }

            DailyEntryService.Delete(selected);
            LoadTodayEntries();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private int GetSearchRank(string foodName, string input)
        {
            string name = Normalize(foodName);

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

            return value
                .ToLowerInvariant()
                .Trim()
                .Replace("ä", "ae")
                .Replace("ö", "oe")
                .Replace("ü", "ue")
                .Replace("ß", "ss");
        }
    }
}