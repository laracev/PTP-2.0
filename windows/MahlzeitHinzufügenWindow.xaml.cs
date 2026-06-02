using Pantry_To_Plate.mods;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Pantry_To_Plate.mods;
using System.Globalization;
namespace Pantry_To_Plate.windows
{
    public partial class MahlzeitHinzufügenWindow : Window
    {
        List<FoodItems> foods = new List<FoodItems>();

        public MahlzeitHinzufügenWindow()
        {
            InitializeComponent();
            LoadCsv();
        }

        void LoadCsv()
        {
            string path = @"data/Lebensmittel.csv";

            if (!File.Exists(path))
            {
                MessageBox.Show("Datei nicht gefunden.");
                return;
            }

            
            var lines = File.ReadAllLines(path, Encoding.Latin1);

            foreach (var line in lines.Skip(1))
            {
                var parts = line.Split(';');

                if (parts.Length > 0 &&
                    !string.IsNullOrWhiteSpace(parts[0]))
                {
                   
                    foods.Add(new FoodItems
                    {
                        //ki start: promt: WTF IST FALSCH ICH NIX CHECKEN DIESE BITTE RETTE MICH AUS DEM VERDAMMEN
                        Name = parts[0],
                        Ballast = parts.Length > 1 ? parts[1] : "",
                        Calories = parts.Length > 2 && double.TryParse(parts[2], NumberStyles.Any, CultureInfo.InvariantCulture, out double calories) ? calories : 0,
                        Protein = parts.Length > 3 && double.TryParse(parts[3], NumberStyles.Any, CultureInfo.InvariantCulture, out double protein) ? protein : 0,
                        Carbs = parts.Length > 4 && double.TryParse(parts[4], NumberStyles.Any, CultureInfo.InvariantCulture, out double carbs) ? carbs : 0,
                        Fat = parts.Length > 5 && double.TryParse(parts[5], NumberStyles.Any, CultureInfo.InvariantCulture, out double fat) ? fat : 0

                        //ki ende
                    });
                }
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string input = TxtBoxLebensmittelHinzufügen.Text.ToLower().Trim();

            ListBoxSuchergebnisse.Items.Clear();
            Btn_LebensMittelHinzufuegen.Content = "";


            if (string.IsNullOrWhiteSpace(input))
            {
                return;
            }

            foreach (FoodItems food in foods)
            {
                string name = food.Name.ToLower();

                if (name.StartsWith(input) ||
                    name.Split(' ').Any(word => word.StartsWith(input)) ||
                    name.Contains(input))
                {
                    ListBoxSuchergebnisse.Items.Add(food.Name);
                }

                if (ListBoxSuchergebnisse.Items.Count >= 10)
                {
                    break;
                }
            }

            if (ListBoxSuchergebnisse.Items.Count == 0)
            {
                Btn_LebensMittelHinzufuegen.Content = "Kein Treffer";
            }
        }

        private void Btn_LebensMittelHinzufuegen_Click(object sender, RoutedEventArgs e)
        {
            if (Btn_LebensMittelHinzufuegen.Content == null)
            {
                return;
            }

            string foodName = Btn_LebensMittelHinzufuegen.Content.ToString();

            if (foodName == "Kein Treffer" || string.IsNullOrWhiteSpace(foodName))
            {
                return;
            }

            if (!double.TryParse(TxtBoxMenge.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double amountGram))
            {
                MessageBox.Show("Bitte eine gültige Menge eingeben.");
                return;
            }

            FoodItems selectedFood = foods.FirstOrDefault(f => f.Name == foodName);

            if (selectedFood == null)
            {
                MessageBox.Show("Lebensmittel wurde nicht gefunden.");
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

            if (!ListBoxLebensmittel.Items.Contains(foodName))
            {
                ListBoxLebensmittel.Items.Add($"{foodName} - {amountGram} g");
            }

            TxtBoxLebensmittelHinzufügen.Clear();
            TxtBoxMenge.Clear();
            ListBoxSuchergebnisse.Items.Clear();
            Btn_LebensMittelHinzufuegen.Content = "";
        }
        private void ListBoxSuchergebnisse_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListBoxSuchergebnisse.SelectedItem != null)
            {
                Btn_LebensMittelHinzufuegen.Content = ListBoxSuchergebnisse.SelectedItem.ToString();
            }
        }
    }
}