using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Imaging;
using NatoliOrderInterface.Models;

namespace NatoliOrderInterface
{
    /// <summary>
    /// Interaction logic for BarcodeLocationWindow.xaml
    /// </summary>
    public partial class BarcodeLocationWindow : Window
    {
        public BarcodeLocationWindow(TravellerScansAudit travellerScansAudit)
        {
            InitializeComponent();
            BitmapImage image;
            string machine = travellerScansAudit.MachineCode.Trim();
            List<string> depts = new List<string>() { "D040", "D080", "D921", "D0043", "D006", "D011", "D1117", "D1151", "D990" };
            bool dept = depts.Contains(travellerScansAudit.DepartmentCode.Trim());
            if (!dept)
            {
                image = new BitmapImage(new Uri(@"Barcode Locations\" + machine + ".png", UriKind.Relative));
            }
            else
            {
                switch (travellerScansAudit.DepartmentCode.Trim())
                {
                    case "D040":
                        image = new BitmapImage(new Uri(@"Barcode Locations\D040_Engineering.png", UriKind.Relative));
                        break;
                    case "D080":
                        image = new BitmapImage(new Uri(@"Barcode Locations\D080_Domestic_Customer_Service.png", UriKind.Relative));
                        break;
                    case "D921":
                        image = new BitmapImage(new Uri(@"Barcode Locations\D921_Production_Management.png", UriKind.Relative));
                        break;
                    case "D0043":
                        image = new BitmapImage(new Uri(@"Barcode Locations\D0043_Chrome_Tank.png", UriKind.Relative));
                        break;
                    case "D006":
                        image = new BitmapImage(new Uri(@"Barcode Locations\D006_Hobbing.png", UriKind.Relative));
                        break;
                    case "D011":
                        image = new BitmapImage(new Uri(@"Barcode Locations\D011_Heat_Treat.png", UriKind.Relative));
                        break;
                    case "D1117":
                        image = new BitmapImage(new Uri(@"Barcode Locations\D1117_Inspection.png", UriKind.Relative));
                        break;
                    case "D1151":
                        image = new BitmapImage(new Uri(@"Barcode Locations\D1151_Order_Entry.png", UriKind.Relative));
                        break;
                    case "D990":
                        image = new BitmapImage(new Uri(@"Barcode Locations\D990_Shipping.png", UriKind.Relative));
                        break;
                    default:
                        image = new BitmapImage(new Uri(@"Barcode Locations\No_Department.png", UriKind.Relative));
                        break;
                }
            }
            
            LocationImage.Source = image;
        }
    }
}
