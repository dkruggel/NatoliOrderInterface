using System.Windows;

namespace NatoliOrderInterface
{
    
    public partial class ProjectSpecificationsWindow : Window
    {
        ProjectWindow projectWindow = null;
        public ProjectSpecificationsWindow(ProjectWindow projectWindow, bool isEnabled, bool NewDrawing, bool UpdateExistingDrawing, bool UpdateTextOnDrawing, bool PerSampleTablet, bool RefTabletDrawing,
                bool PerSampleTool, bool RefToolDrawing, bool PerSuppliedPicture, bool RefNatoliDrawing, bool RefNonNatoliDrawing, string BinLocation)
        {
            if (projectWindow == null)
            {
                Close();
            }
            else
            {
                this.projectWindow = projectWindow;
                InitializeComponent();
                this.NewDrawing.IsChecked = NewDrawing;
                this.NewDrawing.IsEnabled = isEnabled;
                this.UpdateExistingDrawing.IsChecked = UpdateExistingDrawing;
                this.UpdateExistingDrawing.IsEnabled = isEnabled;
                this.UpdateTextOnDrawing.IsChecked = UpdateTextOnDrawing;
                this.UpdateTextOnDrawing.IsEnabled = isEnabled;
                this.PerSampleTablet.IsChecked = PerSampleTablet;
                this.PerSampleTablet.IsEnabled = isEnabled;
                this.RefTabletDrawing.IsChecked = RefTabletDrawing;
                this.RefTabletDrawing.IsEnabled = isEnabled;
                this.PerSampleTool.IsChecked = PerSampleTool;
                this.PerSampleTool.IsEnabled = isEnabled;
                this.RefToolDrawing.IsChecked = RefToolDrawing;
                this.RefToolDrawing.IsEnabled = isEnabled;
                this.PerSuppliedPicture.IsChecked = PerSuppliedPicture;
                this.PerSuppliedPicture.IsEnabled = isEnabled;
                this.RefNatoliDrawing.IsChecked = RefNatoliDrawing;
                this.RefNatoliDrawing.IsEnabled = isEnabled;
                this.RefNonNatoliDrawing.IsChecked = RefNonNatoliDrawing;
                this.RefNonNatoliDrawing.IsEnabled = isEnabled;
                this.BinLocation.Text = string.IsNullOrEmpty(BinLocation) ? "" : BinLocation;
                this.BinLocation.IsEnabled = isEnabled;
                DoneButton.IsEnabled = IsEnabled;
            }
        }

        private void DoneButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
        public bool newDrawing
        {
            get { return NewDrawing.IsChecked ?? false;}
        }
        public bool updateExistingDrawing
        {
            get { return UpdateExistingDrawing.IsChecked ?? false; }
        }
        public bool updateTextOnDrawing
        {
            get { return NewDrawing.IsChecked ?? false; }
        }
        public bool perSampleTablet
        {
            get { return PerSampleTablet.IsChecked ?? false; }
        }
        public bool refTabletDrawing
        {
            get { return RefTabletDrawing.IsChecked ?? false; }
        }
        public bool perSampleTool
        {
            get { return PerSampleTool.IsChecked ?? false; }
        }
        public bool refToolDrawing
        {
            get { return RefToolDrawing.IsChecked ?? false; }
        }
        public bool perSuppliedPicture
        {
            get { return PerSuppliedPicture.IsChecked ?? false; }
        }
        public bool refNatoliDrawing
        {
            get { return RefNatoliDrawing.IsChecked ?? false; }
        }
        public bool refNonNatoliDrawing
        {
            get { return RefNonNatoliDrawing.IsChecked ?? false; }
        }
        public string binLocation
        {
            get { return BinLocation.Text.ToString().Trim(); }
        }
    }
}