using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Pantry_To_Plate.mods
{
    public static class RecipeService
    {
        private static string path = @"data/rezepte_echt_brauchbar_1200.csv";

        public static List<Recipe> Load()
        {
            Directory.CreateDirectory("data");

            if (!File.Exists(path))
            {
                AppLogger.LogWarning("File existiert nicht, Standartfile wird erstellt");
                CreateExampleRecipes();
            }

            List<Recipe> recipes = new List<Recipe>();
            var lines = File.ReadAllLines(path, Encoding.UTF8).Skip(1);

            foreach (string line in lines)
            {
                string[] parts = line.Split(';');

                if (parts.Length < 4 || string.IsNullOrWhiteSpace(parts[0]))
                {
                    AppLogger.Log($"Ungültige Rezeptteile übersprungen");
                    continue;
                }

                Recipe recipe = new Recipe
                {
                    Name = parts[0],
                    Description = parts[1],
                    Ingredients = ParseIngredients(parts[2]),
                    Preparation = parts[3].Replace("<br>", Environment.NewLine)
                };
                AppLogger.Log($"Retzept geladen: {recipe.Name}");

                recipe.MatchPercent = PantryService.CalculateMatchPercent(recipe);
                recipes.Add(recipe);
            }
                
            return recipes
                .OrderByDescending(r => r.MatchPercent)
                .ThenBy(r => PantryService.CalculateMissingGram(r))
                .ThenBy(r => r.Ingredients.Count)
                .ThenBy(r => r.Name)
                .ToList();
        }

        public static void Save(List<Recipe> recipes)
        {
            AppLogger.Log("Rezepte werden gespeichert");
            Directory.CreateDirectory("data");
            List<string> lines = new List<string>();
            lines.Add("Name;Description;Ingredients;Preparation");

            foreach (Recipe recipe in recipes)
            {
                lines.Add(string.Join(";",
                    Clean(recipe.Name),
                    Clean(recipe.Description),
                    SerializeIngredients(recipe.Ingredients),
                    Clean(recipe.Preparation).Replace(Environment.NewLine, "<br>")
                ));
            }

            File.WriteAllLines(path, lines, Encoding.UTF8);
        }

        private static List<Ingredient> ParseIngredients(string text)
        {
            List<Ingredient> ingredients = new List<Ingredient>();

            if (string.IsNullOrWhiteSpace(text))
            {
                return ingredients;
            }

            string[] ingredientParts = text.Split('|');

            foreach (string ingredientText in ingredientParts)
            {
                string[] parts = ingredientText.Split(':');

                if (parts.Length < 2)
                {
                    continue;
                }

                double amount;
                if (!double.TryParse(parts[1].Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out amount))
                {
                    AppLogger.LogWarning($"Ungültige Mengenangae bei Zutat: {parts[0]}");
                    amount = 0;
                }

                ingredients.Add(new Ingredient
                {
                    FoodName = parts[0],
                    AmountGram = amount
                });
            }

            return ingredients;
        }

        private static string SerializeIngredients(List<Ingredient> ingredients)
        {
            if (ingredients == null)
            {
                return "";
            }

            return string.Join("|", ingredients.Select(i => Clean(i.FoodName) + ":" + i.AmountGram.ToString(CultureInfo.InvariantCulture)));
        }

        private static string Clean(string value)
        {
            if (value == null)
            {
                return "";
            }

            return value.Replace(";", ",").Replace("|", ",").Replace(":", " ").Trim();
        }

        private static void CreateExampleRecipes()
        {
            AppLogger.Log("Basisrezepte werden erstellt");
            List<string> lines = new List<string>();
            lines.Add("Name;Description;Ingredients;Preparation");
            lines.Add("Pasta Tomate;Ein schnelles Standardrezept;Nudeln:120|Tomaten:200|Zwiebel:50;Nudeln kochen.<br>Zwiebel anbraten.<br>Tomaten dazugeben und alles mischen.");
            lines.Add("Protein Bowl;Ein einfaches proteinreiches Gericht;Reis:100|Hähnchenbrust:150|Gurke:100|Tomaten:100;Reis kochen.<br>Hähnchen anbraten.<br>Gemüse schneiden und alles in eine Bowl geben.");
            lines.Add("Haferflocken Frühstück;Schnelles Frühstück;Haferflocken:80|Milch:200|Banane:120;Alles zusammen in eine Schüssel geben und kurz ziehen lassen.");
            lines.Add("Gemüse Omelett;Einfaches warmes Gericht;Ei:120|Paprika:100|Zwiebel:40;Gemüse schneiden.<br>Eier verquirlen.<br>Alles in der Pfanne stocken lassen.");
            File.WriteAllLines(path, lines, Encoding.UTF8);
        }
    }
}

