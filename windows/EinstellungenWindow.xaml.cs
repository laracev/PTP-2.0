using Pantry_To_Plate.mods;
using System;
using System.Collections.Generic;
using Pantry_To_Plate.mods;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Globalization;

namespace Pantry_To_Plate.windows
{
    public partial class EinstellungenWindow : Window
    {
        private userinfo userinfo;

        public EinstellungenWindow(userinfo user)
        {
            InitializeComponent();
            userinfo = user;

            Gewichteingabe.Text = userinfo.Weight > 0 ? userinfo.Weight.ToString(CultureInfo.InvariantCulture) : "";
            größeeingabe.Text = userinfo.Height > 0 ? userinfo.Height.ToString(CultureInfo.InvariantCulture) : "";
            ageans.Text = userinfo.Age > 0 ? userinfo.Age.ToString(CultureInfo.InvariantCulture) : "";
            ziel.Text = userinfo.Kalorienziel > 0 ? userinfo.Kalorienziel.ToString(CultureInfo.InvariantCulture) : "";

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
            if (!TryReadPositiveDouble(Gewichteingabe.Text, out double gewicht))
            {
                MessageBox.Show("Bitte gültiges Gewicht eingeben.");
                return;
            }

            if (!TryReadPositiveDouble(größeeingabe.Text, out double größe))
            {
                MessageBox.Show("Bitte gültige Größe eingeben.");
                return;
            }

            if (!TryReadPositiveDouble(ageans.Text, out double alter))
            {
                MessageBox.Show("Bitte gültiges Alter eingeben.");
                return;
            }

            if (Geschlechtcombo.SelectedIndex < 0 || diätzielCombo.SelectedIndex < 0 || AlltagCombo.SelectedIndex < 0)
            {
                MessageBox.Show("Bitte Geschlecht, Diätziel und Alltag auswählen.");
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
            if (!TryReadPositiveDouble(ziel.Text, out double kalorienziel))
            {
                MessageBox.Show("Bitte zuerst ein gültiges Kalorienziel berechnen oder eingeben.");
                return;
            }

            userinfo.Kalorienziel = kalorienziel;

            if (TryReadPositiveDouble(Gewichteingabe.Text, out double gewicht))
            {
                userinfo.Weight = gewicht;
            }

            if (TryReadPositiveDouble(größeeingabe.Text, out double größe))
            {
                userinfo.Height = größe;
            }

            if (TryReadPositiveDouble(ageans.Text, out double alter))
            {
                userinfo.Age = alter;
            }

            userinfo.diätzielChoice = diätzielCombo.SelectedIndex + 1;
            userinfo.Genderchoice = Geschlechtcombo.SelectedIndex + 1;

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
            if (MessageBox.Show("Einstellungen wirklich zurücksetzen?", "Bestätigung", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            diätzielCombo.SelectedIndex = -1;
            AlltagCombo.SelectedIndex = -1;
            Geschlechtcombo.SelectedIndex = -1;
            ageans.Text = "";
            Gewichteingabe.Text = "";
            größeeingabe.Text = "";
            ziel.Text = "";
        }

        private bool TryReadPositiveDouble(string text, out double value)
        {
            return double.TryParse(text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out value) && value > 0;
        }
    }
}