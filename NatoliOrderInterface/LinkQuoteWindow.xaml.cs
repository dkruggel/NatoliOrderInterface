using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NatoliOrderInterface
{
    /// <summary>
    /// Interaction logic for LinkQuoteWindow.xaml
    /// </summary>
    public partial class LinkQuoteWindow : Window
    {

        public LinkQuoteWindow(Window owner)
        {
            Owner = owner;
            InitializeComponent();
        }
        private void PopulateFromQuoteCheckBox_Click(object sender, RoutedEventArgs e)
        {
            switch (((CheckBox)sender).IsChecked)
            {
                case true:
                    TabletOrToolBorder.Visibility = Visibility.Visible;
                    break;
                default:
                    TabletOrToolBorder.Visibility = Visibility.Collapsed;
                    TabletProjectCheckBox.IsChecked = false;
                    ToolProjectCheckBox.IsChecked = false;
                    break;
            }
        }

        private void FinishButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        public bool PopulateFromQuote
        {
            get { return PopulateFromQuoteCheckBox.IsChecked ?? false; }
        }
        public bool TabletProject
        {
            get { return TabletProjectCheckBox.IsChecked ?? false; }
        }
        public bool ToolProject
        {
            get { return ToolProjectCheckBox.IsChecked ?? false; }
        }
    }
}
