using Pantry_To_Plate.mods;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Pantry_To_Plate.windows
{
    public partial class RezepteWindow : Window
    {
        private List<Recipe> recipes = new List<Recipe>();
        private List<FoodItems> foods = new List<FoodItems>();
        private Recipe selectedRecipe;

        public RezepteWindow()
        {
            InitializeComponent();
            foods = FoodCatalogService.LoadAll();
            LoadRecipes();
        }

        private void LoadRecipes()
        {
            recipes = RecipeService.Load();
            ListBoxRecipes.Items.Clear();

            foreach (Recipe recipe in recipes)
            {
                ListBoxRecipes.Items.Add(recipe);
            }

            if (ListBoxRecipes.Items.Count > 0)
            {
                ListBoxRecipes.SelectedIndex = 0;
            }
        }

        private void ListBoxRecipes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedRecipe = ListBoxRecipes.SelectedItem as Recipe;
            ShowRecipe(selectedRecipe);
        }

        private void ShowRecipe(Recipe recipe)
        {
            if (recipe == null)
            {
                return;
            }

            TxtRecipeName.Text = recipe.Name;
            TxtRecipeDescription.Text = recipe.Description;
            TxtRecipeMatch.Text = $"{recipe.MatchPercent:F0}% der Zutaten vollständig vorhanden";
            TxtPreparation.Text = recipe.Preparation;

            DataGridIngredients.ItemsSource = null;
            DataGridIngredients.ItemsSource = recipe.Ingredients;

            ListBoxMissing.Items.Clear();
            List<Ingredient> missing = PantryService.GetMissingIngredients(recipe);

            if (missing.Count == 0)
            {
                ListBoxMissing.Items.Add("Alles vorhanden");
            }
            else
            {
                foreach (Ingredient ingredient in missing)
                {
                    ListBoxMissing.Items.Add(ingredient.ToString());
                }
            }
        }

        private void AddMissingToShoppingList_Click(object sender, RoutedEventArgs e)
        {
            if (selectedRecipe == null)
            {
                MessageBox.Show("Bitte zuerst ein Rezept auswählen.");
                return;
            }

            List<Ingredient> missing = PantryService.GetMissingIngredients(selectedRecipe);

            if (missing.Count == 0)
            {
                MessageBox.Show("Für dieses Rezept fehlt nichts.");
                return;
            }

            ShoppingListService.AddMissingIngredients(missing);
            MessageBox.Show("Fehlende Zutaten wurden zur Einkaufsliste hinzugefügt.");
        }

        private void CookRecipe_Click(object sender, RoutedEventArgs e)
        {
            if (selectedRecipe == null)
            {
                MessageBox.Show("Bitte zuerst ein Rezept auswählen.");
                return;
            }

            List<Ingredient> missing = PantryService.GetMissingIngredients(selectedRecipe);

            if (missing.Count > 0)
            {
                MessageBox.Show("Du hast nicht alle Zutaten im Pantry. Fehlende Zutaten können zur Einkaufsliste hinzugefügt werden.");
                return;
            }

            PantryService.ConsumeIngredients(selectedRecipe);
            DailyEntryService.Add(selectedRecipe.ToDailyEntry(foods));
            MessageBox.Show("Rezept wurde als Mahlzeit gespeichert und die Zutaten wurden aus dem Pantry abgezogen.");
            LoadRecipes();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
