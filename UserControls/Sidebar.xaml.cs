using Pantry_To_Plate.mods;
using Pantry_To_Plate.windows;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Pantry_To_Plate.UserControls
{
    public partial class Sidebar : UserControl
    {
        public static readonly DependencyProperty ActivePageProperty = DependencyProperty.Register(
            "ActivePage",
            typeof(string),
            typeof(Sidebar),
            new PropertyMetadata("", OnActivePageChanged));

        public string ActivePage
        {
            get { return (string)GetValue(ActivePageProperty); }
            set { SetValue(ActivePageProperty, value); }
        }

        public Sidebar()
        {
            InitializeComponent();
            Loaded += Sidebar_Loaded;
        }

        private void Sidebar_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateActiveButton();
        }

        private static void OnActivePageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Sidebar sidebar = d as Sidebar;
            if (sidebar != null)
            {
                sidebar.UpdateActiveButton();
            }
        }

        private void UpdateActiveButton()
        {
            if (BtnHome == null)
            {
                return;
            }

            BtnHome.Style = (Style)FindResource("NavButtonStyle");
            BtnPantry.Style = (Style)FindResource("NavButtonStyle");
            BtnRecipes.Style = (Style)FindResource("NavButtonStyle");
            BtnMeals.Style = (Style)FindResource("NavButtonStyle");
            BtnShopping.Style = (Style)FindResource("NavButtonStyle");
            BtnFitness.Style = (Style)FindResource("NavButtonStyle");
            BtnSettings.Style = (Style)FindResource("NavButtonStyle");

            Button activeButton = null;

            switch ((ActivePage ?? "").ToLowerInvariant())
            {
                case "home":
                    activeButton = BtnHome;
                    break;
                case "pantry":
                    activeButton = BtnPantry;
                    break;
                case "rezepte":
                case "recipes":
                    activeButton = BtnRecipes;
                    break;
                case "mahlzeiten":
                case "meals":
                    activeButton = BtnMeals;
                    break;
                case "einkaufsliste":
                case "shopping":
                    activeButton = BtnShopping;
                    break;
                case "fitness":
                    activeButton = BtnFitness;
                    break;
                case "einstellungen":
                case "settings":
                    activeButton = BtnSettings;
                    break;
            }

            if (activeButton != null)
            {
                activeButton.Style = (Style)FindResource("ActiveNavButtonStyle");
            }
        }

        private void Home_Click(object sender, RoutedEventArgs e)
        {
            Window current = Window.GetWindow(this);

            if (current is MainWindow)
            {
                return;
            }

            if (current != null && current.Owner is MainWindow)
            {
                current.Owner.Show();
                current.Owner.WindowState = WindowState.Maximized;
                current.Owner.Activate();
                current.Close();
                return;
            }

            OpenWindow(new MainWindow());
        }

        private void Pantry_Click(object sender, RoutedEventArgs e)
        {
            OpenWindow(new PantryWindow());
        }

        private void Recipes_Click(object sender, RoutedEventArgs e)
        {
            OpenWindow(new RezepteWindow());
        }

        private void Meals_Click(object sender, RoutedEventArgs e)
        {
            OpenWindow(new MahlzeitHinzufügenWindow());
        }

        private void Shopping_Click(object sender, RoutedEventArgs e)
        {
            OpenWindow(new EinkaufslisteWindow());
        }

        private void Fitness_Click(object sender, RoutedEventArgs e)
        {
            OpenWindow(new fitnessaktivitäthinzufügenwindow());
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            OpenWindow(new EinstellungenWindow(UserDataService.Load()));
        }

        private void OpenWindow(Window newWindow)
        {
            Window current = Window.GetWindow(this);

            if (current != null && current.GetType() == newWindow.GetType())
            {
                return;
            }

            Window owner = current != null ? current.Owner : null;

            newWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            newWindow.WindowState = WindowState.Maximized;

            if (owner != null && owner != newWindow)
            {
                newWindow.Owner = owner;
            }

            newWindow.Show();

            if (current is MainWindow)
            {
                current.Hide();
            }
            else if (current != null)
            {
                current.Close();
            }
        }
    }
}
