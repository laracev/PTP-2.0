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
            // Fenster fade in
            DoubleAnimation fade = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.5));
            this.BeginAnimation(Window.OpacityProperty, fade);

            // Logo fade in and zoom in
            DoubleAnimation logoFade = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.6));
            Logo.BeginAnimation(UIElement.OpacityProperty, logoFade);

            DoubleAnimation zoom = new DoubleAnimation(0.7, 1.0, TimeSpan.FromSeconds(0.6));
            LogoScale.BeginAnimation(ScaleTransform.ScaleXProperty, zoom);
            LogoScale.BeginAnimation(ScaleTransform.ScaleYProperty, zoom);

            await Task.Delay(1200);

            new MainWindow().Show();
            this.Close();
            //ki end
        }
    }
}
