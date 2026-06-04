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
        // Hinweis: Diese Datei wurde bearbeitet von KI (GitHub Copilot)
        // ki start
        // Prompt: Zeige Animation beim Start, Logo soll beim Fade-In sichtbar werden
        

        public AnimationWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Dauer der Animationen (sekunden)
            double animationSeconds = 1.0;
            TimeSpan animDuration = TimeSpan.FromSeconds(animationSeconds);

            // Fenster fade in
            DoubleAnimation fade = new DoubleAnimation(0, 1, animDuration);
            this.BeginAnimation(Window.OpacityProperty, fade);

            // Logo fade in and zoom in (safe: check for null and ensure ScaleTransform exists)
            DoubleAnimation logoFade = new DoubleAnimation(0, 1, animDuration);
            DoubleAnimation zoom = new DoubleAnimation(0.7, 1.0, animDuration);

            if (Logo != null)
            {
                Logo.BeginAnimation(UIElement.OpacityProperty, logoFade);

                // ensure a ScaleTransform is available on the Logo
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
                AppLogger.LogWarning("AnimationWindow: Logo element not found in XAML (Logo is null).");
            }

            // Start loading pantry CSV in background while animation plays
            var loadTask = Task.Run(() =>
            {
                try
                {
                    PantryService.Load();
                }
                catch (Exception ex)
                {
                    AppLogger.LogError($"Fehler beim Laden der Pantry: {ex.Message}");
                }
            });

            // Wartezeit messen: warte auf das Laden und sorge dafür, dass die Animation
            // mindestens so lange läuft wie angegeben (animDuration). Timeline.Completed
            // kann in einigen Fällen nicht auf dem ursprünglichen Timeline-Objekt feuern,
            // daher verwenden wir hier eine Zeitbasierte Lösung.
            var start = DateTime.UtcNow;
            await loadTask; // warte bis CSV geladen ist

            var elapsed = DateTime.UtcNow - start;
            var remaining = animDuration - elapsed;
            if (remaining > TimeSpan.Zero)
            {
                await Task.Delay(remaining);
            }

            new MainWindow().Show();
            this.Close();
            //ki end
        }
    }
}
