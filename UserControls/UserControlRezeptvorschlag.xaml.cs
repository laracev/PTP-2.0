using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Pantry_To_Plate.mods;
using Pantry_To_Plate.windows;

namespace Pantry_To_Plate.UserControls
{
    public partial class UserControlRezeptvorschlag : UserControl
    {
        private Recipe suggestedRecipe;

        public UserControlRezeptvorschlag()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSuggestedRecipe();
        }

        private void LoadSuggestedRecipe()
        {
            List<Recipe> recipes = RecipeService.Load();
            suggestedRecipe = recipes.FirstOrDefault(r => r.MatchPercent > 0) ?? recipes.FirstOrDefault();

            if (suggestedRecipe == null)
            {
                TxtRecipeName.Text = "Kein Rezept gefunden";
                TxtPercent.Text = "0%";
                TxtInfo.Text = "Lege zuerst Rezepte an, damit ein Vorschlag angezeigt werden kann.";
                ProgressMatch.Value = 0;
                BtnOpenRecipes.IsEnabled = false;
                return;
            }

            List<Ingredient> missing = PantryService.GetMissingIngredients(suggestedRecipe);
            double missingGram = missing.Sum(i => i.AmountGram);

            TxtRecipeName.Text = suggestedRecipe.Name;
            TxtPercent.Text = $"{suggestedRecipe.MatchPercent:F0}%";
            ProgressMatch.Value = Math.Max(0, Math.Min(100, suggestedRecipe.MatchPercent));
            BtnOpenRecipes.IsEnabled = true;

            if (missing.Count == 0)
            {
                TxtInfo.Text = "Du hast alle Zutaten dafür im Pantry.";
            }
            else
            {
                string firstMissing = string.Join(", ", missing.Take(2).Select(i => $"{i.FoodName} ({i.AmountGram:F0}g)"));
                string moreText = missing.Count > 2 ? $" + {missing.Count - 2} weitere" : "";
                TxtInfo.Text = $"Fehlt noch: {firstMissing}{moreText}. Insgesamt fehlen {missingGram:F0}g.";
            }
        }

        private void BtnOpenRecipes_Click(object sender, RoutedEventArgs e)
        {
            RezepteWindow window = new RezepteWindow();
            Window owner = Window.GetWindow(this);

            if (owner != null)
            {
                window.Owner = owner;
            }

            window.ShowDialog();
            LoadSuggestedRecipe();
        }
    }
}
