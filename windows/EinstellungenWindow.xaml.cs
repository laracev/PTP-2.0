using Pantry_To_Plate.mods;
using System;
using System.Collections.Generic;
using Pantry_To_Plate.mods;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Pantry_To_Plate.windows
{
    public partial class EinstellungenWindow : Window
    {
        private userinfo userinfo;

        public EinstellungenWindow(userinfo user)
        {
            InitializeComponent();
            userinfo = user;

            Gewichteingabe.Text = userinfo.Weight.ToString();
            größeeingabe.Text = userinfo.Height.ToString();
            ageans.Text = userinfo.Age.ToString();
            ziel.Text = userinfo.Kalorienziel.ToString();

            Geschlechtcombo.SelectedIndex = userinfo.Genderchoice - 1;
            diätzielCombo.SelectedIndex = userinfo.diätzielChoice - 1;


        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Empfehlung berechnen und einfügen
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(Gewichteingabe.Text, out double gewicht))
            {
                MessageBox.Show("Bitte gültiges Gewicht eingeben.");
                return;
            }

            if (!double.TryParse(größeeingabe.Text, out double größe))
            {
                MessageBox.Show("Bitte gültige Größe eingeben.");
                return;
            }

            if (!double.TryParse(ageans.Text, out double alter))
            {
                MessageBox.Show("Bitte gültiges Alter eingeben.");
                return;
            }

            userinfo.Weight = gewicht;
            userinfo.Height = größe;
            userinfo.Age = alter;

            userinfo.diätzielChoice = diätzielCombo.SelectedIndex + 1;
            userinfo.Genderchoice = Geschlechtcombo.SelectedIndex + 1;

            userinfo.palWert = AlltagCombo.SelectedIndex switch
            {
                0 => 1.2,
                1 => 1.3,
                2 => 1.5,
                3 => 1.7,
                4 => 1.9,
                5 => 2.2,
                _ => 1.5
            };

            double kcal = userinfo.KcalZielBerechnen(
                userinfo.Weight,
                userinfo.Height,
                userinfo.diätzielChoice,
                userinfo.Genderchoice,
                userinfo.Age
            );

            userinfo.Kalorienziel = kcal;
            ziel.Text = Math.Round(kcal, 0).ToString();
        }

        // Werte speichern und schließen
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(ziel.Text, out double kalorienziel))
            {
                userinfo.Kalorienziel = kalorienziel;
            }

            UserDataService.Save(userinfo);

            MessageBox.Show("Einstellungen wurden gespeichert.");
            this.Close();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Diese Funktion ist noch nicht eingebaut.");
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e) { }
        private void TextBox_TextChanged_1(object sender, TextChangedEventArgs e) { }
        private void TextBox_TextChanged_2(object sender, TextChangedEventArgs e) { }
        private void TextBox_TextChanged_3(object sender, TextChangedEventArgs e) { }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) { }
        private void ComboBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e) { }
        private void ComboBox_SelectionChanged_2(object sender, SelectionChangedEventArgs e) { }

        private void BtnEinstellungenZurücksetzen_Click(object sender, RoutedEventArgs e)
        {
            diätzielCombo.SelectedIndex = -1;
            AlltagCombo.SelectedIndex = -1;
            Geschlechtcombo.SelectedIndex = -1;
            ageans.Text = "";
            Gewichteingabe.Text = "";
            größeeingabe.Text = "";
            ziel.Text = "";
        }
    }
}