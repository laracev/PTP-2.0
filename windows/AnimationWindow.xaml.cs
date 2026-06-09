using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Pantry_To_Plate.mods;

namespace Pantry_To_Plate.windows
{
    /// <summary>
    /// Interaktionslogik für AnimationWindow.xaml
    /// </summary>
    public partial class AnimationWindow : Window
    {
        public AnimationWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Animation länger anzeigen
            double animationSeconds = 3.0;
            TimeSpan animDuration = TimeSpan.FromSeconds(animationSeconds);

            // Alles direkt beim Start parallel im Hintergrund laden,
            // während die Animation läuft.
            Task loadTask = Task.Run(() =>
            {
                try
                {
                    PantryService.Load();
                    ShoppingListService.Load();
                    RecipeService.Load();
                    UserDataService.Load();
                    FoodCatalogService.LoadAll();
                }
                catch (Exception ex)
                {
                    AppLogger.LogError($"Fehler beim Vorladen der Daten: {ex.Message}");
                }
            });

            // Fenster fade in
            DoubleAnimation fade = new DoubleAnimation(0, 1, animDuration);
            this.BeginAnimation(Window.OpacityProperty, fade);

            // Logo fade in und zoom in
            DoubleAnimation logoFade = new DoubleAnimation(0, 1, animDuration);
            DoubleAnimation zoom = new DoubleAnimation(0.7, 1.0, animDuration);

            if (Logo != null)
            {
                Logo.BeginAnimation(UIElement.OpacityProperty, logoFade);

                ScaleTransform scale = null;
                if (Logo.RenderTransform is ScaleTransform st)
                {
                    scale = st;
                }
                else
                {
                    scale = new ScaleTransform(1, 1);
                    Logo.RenderTransform = scale;
                    Logo.RenderTransformOrigin = new Point(0.5, 0.5);
                }

                scale.BeginAnimation(ScaleTransform.ScaleXProperty, zoom);
                scale.BeginAnimation(ScaleTransform.ScaleYProperty, zoom);
            }
            else
            {
                AppLogger.LogWarning("AnimationWindow: Logo element not found in XAML.");
            }

            // Animation mindestens 3 Sekunden anzeigen und gleichzeitig auf Ladevorgang warten.
            await Task.WhenAll(loadTask, Task.Delay(animDuration));

            new MainWindow().Show();
            this.Close();
        }
    }
}

