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
    public partial class CoreRodWindow : Window ,IMethods
    {
        public CoreRodWindow(Window owner, bool isEnabled ,
            bool coreRod, string coreRodSteelID,
            bool coreRodKey, string coreRodKeySteelID,
            bool coreRodKeyCollar, string coreRodKeyCollarSteelID,
            bool coreRodPunch, string coreRodPunchSteelID)
        {
            Owner = owner;
            InitializeComponent();
            LowerCoreRod.IsChecked = coreRod;
            LowerCoreRodSteelID.Text = coreRodSteelID ?? "";
            LowerCoreRodKey.IsChecked = coreRodKey;
            LowerCoreRodKeySteelID.Text = coreRodKeySteelID ?? "";
            LowerCoreRodKeyCollar.IsChecked = coreRodKeyCollar;
            LowerCoreRodKeyCollarSteelID.Text = coreRodKeyCollarSteelID ?? "";
            LowerCoreRodPunch.IsChecked = coreRodPunch;
            LowerCoreRodPunchSteelID.Text = coreRodPunchSteelID ?? "";
            LowerCoreRodSteelID.ItemsSource = IMethods.GetSteelIDItemsSource();
            LowerCoreRodKeySteelID.ItemsSource = IMethods.GetSteelIDItemsSource();
            LowerCoreRodKeyCollarSteelID.ItemsSource = IMethods.GetSteelIDItemsSource();
            LowerCoreRodPunchSteelID.ItemsSource = IMethods.GetSteelIDItemsSource();

            LowerCoreRodPunchSteelID.IsEnabled = IsEnabled;
            LowerCoreRodPunch.IsEnabled = IsEnabled;
            LowerCoreRodKeyCollarSteelID.IsEnabled = IsEnabled;
            LowerCoreRodKeyCollar.IsEnabled = IsEnabled;
            LowerCoreRodKeySteelID.IsEnabled = IsEnabled;
            LowerCoreRodKey.IsEnabled = IsEnabled;
            LowerCoreRodSteelID.IsEnabled = IsEnabled;
            LowerCoreRod.IsEnabled = IsEnabled;
            DoneButton.IsEnabled = IsEnabled;
        }

        private void DoneButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
        public bool CoreRod
        {
            get { return LowerCoreRod.IsChecked ?? false; }
        }
        public string CoreRodSteelID
        {
            get { return string.IsNullOrWhiteSpace(LowerCoreRodSteelID.Text) ? null : LowerCoreRodSteelID.Text.Trim(); }
        }
        public bool CoreRodKey
        {
            get { return LowerCoreRodKey.IsChecked ?? false; }
        }
        public string CoreRodKeySteelID
        {
            get { return string.IsNullOrWhiteSpace(LowerCoreRodKeySteelID.Text) ? null : LowerCoreRodKeySteelID.Text.Trim(); }
        }
        public bool CoreRodKeyCollar
        {
            get { return LowerCoreRodKeyCollar.IsChecked ?? false; }
        }
        public string CoreRodKeyCollarSteelID
        {
            get { return string.IsNullOrWhiteSpace(LowerCoreRodKeyCollarSteelID.Text) ? null : LowerCoreRodKeyCollarSteelID.Text.Trim(); }
        }
        public bool CoreRodPunch
        {
            get { return LowerCoreRodKey.IsChecked ?? false; }
        }
        public string CoreRodPunchSteelID
        {
            get { return string.IsNullOrWhiteSpace(LowerCoreRodPunchSteelID.Text) ? null : LowerCoreRodPunchSteelID.Text.Trim(); }
        }
    }
}
