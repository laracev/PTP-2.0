using Pantry_To_Plate.mods;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace Pantry_To_Plate.windows
{
    public partial class PantryWindow : Window
    {
        private List<FoodItems> foods = new List<FoodItems>();
        private List<PantryItem> pantry = new List<PantryItem>();
        private bool isSelectingFood = false;

        public PantryWindow()
        {
            InitializeComponent();
            foods = FoodCatalogService.LoadAll();
            LoadPantry();
            ShowFoodSuggestions("");
        }

        private void LoadPantry()
        {
            pantry = PantryService.Load();
            DataGridPantry.ItemsSource = null;
            DataGridPantry.ItemsSource = pantry;
            TxtSummary.Text = $"{pantry.Count} Einträge";
        }

        private void ShowFoodSuggestions(string searchText)
        {
            ListBoxFoods.Items.Clear();

            string input = Normalize(searchText);
            IEnumerable<FoodItems> query = foods;

            if (!string.IsNullOrWhiteSpace(input))
            {
                query = foods
                    .Where(f => !string.IsNullOrWhiteSpace(f.Name) && Normalize(f.Name).Contains(input))
                    .OrderBy(f => GetSearchRank(f.Name, input))
                    .ThenBy(f => f.Name.Length)
                    .ThenBy(f => f.Name);
            }
            else
            {
                query = foods.OrderBy(f => f.Name);
            }

            foreach (FoodItems food in query.Take(25))
            {
                ListBoxFoods.Items.Add(food.Name);
            }
        }

        private int GetSearchRank(string foodName, string input)
        {
            string name = Normalize(foodName);

            if (name == input)
            {
                return 0;
            }

            if (name.StartsWith(input + " ") || name.StartsWith(input + "-") || name.StartsWith(input + "/") || name.StartsWith(input))
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

            string normalized = value.ToLowerInvariant().Trim();
            normalized = normalized.Replace("ä", "ae").Replace("ö", "oe").Replace("ü", "ue").Replace("ß", "ss");
            normalized = Regex.Replace(normalized, @"\s+", " ");
            return normalized;
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!isSelectingFood)
            {
                ShowFoodSuggestions(TxtSearch.Text);
            }
        }

        private void ListBoxFoods_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListBoxFoods.SelectedItem != null)
            {
                isSelectingFood = true;
                TxtSearch.Text = ListBoxFoods.SelectedItem.ToString();
                TxtSearch.CaretIndex = TxtSearch.Text.Length;
                isSelectingFood = false;
            }
        }

        private void AddToPantry_Click(object sender, RoutedEventArgs e)
        {
            string foodName = TxtSearch.Text.Trim();

            if (string.IsNullOrWhiteSpace(foodName))
            {
                MessageBox.Show("Bitte ein Lebensmittel auswählen oder eingeben.");
                return;
            }

            double amountGram;
            if (!double.TryParse(TxtAmount.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out amountGram) || amountGram <= 0)
            {
                MessageBox.Show("Bitte eine gültige Menge in Gramm eingeben.");
                return;
            }

            FoodItems selectedFood = foods.FirstOrDefault(f => string.Equals(f.Name, foodName, StringComparison.OrdinalIgnoreCase));

            if (selectedFood == null)
            {
                MessageBox.Show("Dieses Lebensmittel wurde in der Lebensmittel.csv nicht gefunden.");
                return;
            }

            PantryService.AddOrUpdate(selectedFood, amountGram);
            TxtAmount.Clear();
            LoadPantry();
        }

        private void DeleteSelected_Click(object sender, RoutedEventArgs e)
        {
            PantryItem selected = DataGridPantry.SelectedItem as PantryItem;

            if (selected == null)
            {
                MessageBox.Show("Bitte zuerst einen Pantry-Eintrag auswählen.");
                return;
            }

            pantry.Remove(selected);
            PantryService.Save(pantry);
            LoadPantry();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

