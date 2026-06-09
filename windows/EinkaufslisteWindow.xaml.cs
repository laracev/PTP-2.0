using Pantry_To_Plate.mods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace Pantry_To_Plate.windows
{
    /// <summary>
    /// Interaktionslogik für EinkaufslisteWindow.xaml
    /// </summary>
    public partial class EinkaufslisteWindow : Window
    {
        private List<Ingredient> shoppingList = new List<Ingredient>();
        private List<FoodItems> foods = new List<FoodItems>();

        public EinkaufslisteWindow()
        {
            InitializeComponent();
            foods = FoodCatalogService.LoadAll();
            LoadShoppingList();
        }

        private void LoadShoppingList()
        {
            shoppingList = ShoppingListService.Load();
            DataGridShoppingList.ItemsSource = null;
            DataGridShoppingList.ItemsSource = shoppingList;
        }

        private void BuySelected_Click(object sender, RoutedEventArgs e)
        {
            Ingredient selected = DataGridShoppingList.SelectedItem as Ingredient;

            if (selected == null)
            {
                MessageBox.Show("Bitte zuerst einen Eintrag aus der Einkaufsliste auswählen.");
                return;
            }

            FoodItems food = FindFoodForShoppingItem(selected.FoodName);

            if (food == null)
            {
                MessageBox.Show("Dieses Lebensmittel wurde in der Lebensmittel.csv nicht gefunden und kann nicht ins Pantry übernommen werden.");
                return;
            }

            PantryService.AddOrUpdate(food, selected.AmountGram);
            shoppingList.Remove(selected);
            ShoppingListService.Save(shoppingList);
            LoadShoppingList();

            MessageBox.Show($"{selected.FoodName} wurde eingekauft und ins Pantry hinzugefügt.");
        }

        private void BuyAll_Click(object sender, RoutedEventArgs e)
        {
            if (shoppingList.Count == 0)
            {
                MessageBox.Show("Die Einkaufsliste ist leer.");
                return;
            }

            List<Ingredient> notFound = new List<Ingredient>();

            foreach (Ingredient item in shoppingList.ToList())
            {
                FoodItems food = FindFoodForShoppingItem(item.FoodName);

                if (food == null)
                {
                    notFound.Add(item);
                    continue;
                }

                PantryService.AddOrUpdate(food, item.AmountGram);
                shoppingList.Remove(item);
            }

            ShoppingListService.Save(shoppingList);
            LoadShoppingList();

            if (notFound.Count > 0)
            {
                MessageBox.Show($"Einige Lebensmittel wurden nicht in der Lebensmittel.csv gefunden und bleiben auf der Einkaufsliste: {string.Join(", ", notFound.Select(i => i.FoodName))}");
            }
            else
            {
                MessageBox.Show("Alle Lebensmittel wurden eingekauft und ins Pantry hinzugefügt.");
            }
        }

        private FoodItems FindFoodForShoppingItem(string foodName)
        {
            string normalizedInput = Normalize(foodName);

            FoodItems exact = foods.FirstOrDefault(f => Normalize(f.Name) == normalizedInput);

            if (exact != null)
            {
                return exact;
            }

            string singularInput = normalizedInput.EndsWith("n") ? normalizedInput.Substring(0, normalizedInput.Length - 1) : normalizedInput;

            return foods.FirstOrDefault(f =>
                Normalize(f.Name).StartsWith(singularInput + " ") ||
                Normalize(f.Name).StartsWith(singularInput + "-") ||
                Normalize(f.Name).StartsWith(singularInput + "/") ||
                singularInput.StartsWith(Normalize(f.Name)));
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

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            ShoppingListService.Clear();
            LoadShoppingList();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Sidebar_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
