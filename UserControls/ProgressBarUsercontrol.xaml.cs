using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Pantry_To_Plate.UserControls
{
    /// <summary>
    /// Interaktionslogik für UserControl1.xaml
    /// </summary>
    public partial class UserControl1 : UserControl
    {
        public UserControl1()
        {
            InitializeComponent();
        }

        public void UpdateBar(double gegessenKalorien, double kalorienZiel)
        {
            if (kalorienZiel <= 0)
            {
                AnimateWidth(0);
                return;
            }

            double uebrig = kalorienZiel - gegessenKalorien;
            double prozent = uebrig / kalorienZiel;

            if (prozent < 0)
            {
                prozent = 0;
            }

            if (prozent > 1)
            {
                prozent = 1;
            }

            if (uebrig < 0)
            {
                ProgressRect.Fill = new SolidColorBrush(Color.FromRgb(235, 71, 123));
            }
            else
            {
                ProgressRect.Fill = new SolidColorBrush(Color.FromRgb(100, 140, 88));
            }

            double neueBreite = 400 * prozent;
            AnimateWidth(neueBreite);
        }
        //Chatgpt start
        private void AnimateWidth(double neueBreite)
        {
            DoubleAnimation animation = new DoubleAnimation();
            animation.To = neueBreite;
            animation.Duration = TimeSpan.FromMilliseconds(500);
            animation.EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut };

            ProgressRect.BeginAnimation(WidthProperty, animation);
        }
        //chatGPT ende
    }
}
