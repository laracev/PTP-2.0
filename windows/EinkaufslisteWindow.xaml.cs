using Pantry_To_Plate.mods;
using System;
using System.Collections.Generic;
using System.Security.Policy;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Pantry_To_Plate.windows
{
    /// <summary>
    /// Interaktionslogik für EinkaufslisteWindow.xaml
    /// </summary>
    public partial class EinkaufslisteWindow : Window
    {
        
        public EinkaufslisteWindow()
        {
            InitializeComponent();
            LoadShoppingList();
            
        }

        private void LoadShoppingList()
        {
            DataGridShoppingList.ItemsSource = null;
            DataGridShoppingList.ItemsSource = ShoppingListService.Load();
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            ShoppingListService.Clear();
            LoadShoppingList();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
