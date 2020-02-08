using NatoliOrderInterface.Models;
using System;
using System.Linq;
using System.Windows;

namespace NatoliOrderInterface
{
    /// <summary>
    /// Interaction logic for DocumentTrackingWindow.xaml
    /// </summary>
    public partial class DocumentTrackingWindow : Window
    {
        private User user;
        private WorkOrder workOrder;
        private Quote quote;
        private string documentType;
        private string documentNumber;
        public DocumentTrackingWindow(User _user)
        {
            user = _user;
            InitializeComponent();
            OtherLocationsComboBox.Items.Add("Production Management");
            OtherLocationsComboBox.Items.Add("Shipped");
        }

        public DocumentTrackingWindow(WorkOrder order, User _user)
        {
            user = _user;
            workOrder = order;
            Title = "Order Tracking for Order: " + workOrder.OrderNumber.ToString();
            InitializeComponent();
            OtherLocationsComboBox.Items.Add("Production Management");
            OtherLocationsComboBox.Items.Add("Shipped");
            documentType = "Order";
            documentNumber = workOrder.OrderNumber.ToString();
        }

        public DocumentTrackingWindow(Quote quote, User _user)
        {
            user = _user;
            this.quote = quote;
            Title = "Order Tracking for Quote: " + this.quote.QuoteNumber.ToString();
            InitializeComponent();
            OtherLocationsComboBox.Items.Add("Production Management");
            OtherLocationsComboBox.Items.Add("Shipped");
            documentType = "Quote";
            documentNumber = this.quote.QuoteNumber.ToString() + '-' + this.quote.QuoteRevNo.ToString();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Initiate an instance of the context
            using var _ = new NAT02Context();

            try
            {
                EoiTrackedDocuments trackedDocument = new EoiTrackedDocuments();

                if (ToProductionCheckBox.IsChecked.Value)
                {
                    // Insert into EOI_TrackedDocuments
                    trackedDocument.Type = documentType; // Quote or Order
                    trackedDocument.Number = documentNumber; // Quote number or Order number
                    trackedDocument.MovementId = 3; // Type of movement to notify for
                    trackedDocument.User = user.GetUserName(); // User requesting notification

                    // Execute the DML statement
                    _.EoiTrackedDocuments.Add(trackedDocument);
                }

                if (ShippedCheckBox.IsChecked.Value)
                {
                    // Insert into EOI_TrackedDocuments
                    trackedDocument.Type = documentType; // Quote or Order
                    trackedDocument.Number = documentNumber; // Quote number and rev or Order number
                    trackedDocument.MovementId = 5; // Type of movement to notify for
                    trackedDocument.User = user.GetUserName(); // User requesting notification

                    // Execute the DML statement
                    _.EoiTrackedDocuments.Add(trackedDocument);
                }

                // Save the changes
                int ret = _.SaveChanges();

                if (ret == 1)
                {
                    MessageBox.Show("Document will now be tracked.");
                }
                else
                {
                    throw new Microsoft.EntityFrameworkCore.DbUpdateException();
                }
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("SaveButton_Click -- DocumentTrackingWindow.xaml.cs", ex.Message, user);
            }
            finally
            {
                // Dispose of the context object
                _.Dispose();
            }
        }
    }
}
