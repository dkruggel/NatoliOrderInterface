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
    /// Interaction logic for InputBox.xaml
    /// </summary>
    public partial class InputBox : Window
    {
        public string ReturnString { get; set; }
        private string promptText;
        TextBox ReturnTextBox = new TextBox()
        {
            VerticalAlignment = VerticalAlignment.Center,
            Width = 120
        };
        PasswordBox PasswordTextBox = new PasswordBox()
        {
            VerticalAlignment = VerticalAlignment.Center,
            Width = 120
        };

        public InputBox()
        {
            InitializeComponent();
        }

        public InputBox(string prompt, string title, Window owner)
        {
            this.Owner = owner;
            Title = title;
            InitializeComponent();
            InputTextLabel.Content = prompt;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            if (title == "Password")
            {
                StackPanel1.Children.Add(PasswordTextBox);
                PasswordTextBox.Focus();
                PasswordTextBox.PreviewKeyUp += PasswordTextBox_PreviewKeyUp;
            }
            else
            {
                StackPanel1.Children.Add(ReturnTextBox);
                ReturnTextBox.Focus();
                ReturnTextBox.PreviewKeyUp += ReturnTextBox_PreviewKeyUp;
            }
        }

        private void ReturnTextBox_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ReturnString = ReturnTextBox.Text;
                Close();
            }
        }

        private void PasswordTextBox_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ReturnString = PasswordTextBox.Password;
                Close();
            }
        }

        private void OkayButton_Click(object sender, RoutedEventArgs e)
        {
            if (ReturnTextBox.Text.Length > 0)
            {
                ReturnString = ReturnTextBox.Text;
            }
            else if (PasswordTextBox.Password.Length > 0)
            {
                ReturnString = PasswordTextBox.Password;
            }
            else
            {
                ReturnString = "";
            }
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            ReturnString = "";
            Close();
        }
    }
}
