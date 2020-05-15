using NatoliOrderInterface.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;
using System.Linq;
using NatoliOrderInterface.Models.NAT01;
using NatoliOrderInterface.Models.Projects;
using NatoliOrderInterface.Models.DriveWorks;

namespace NatoliOrderInterface
{
    class BackgroundConverter : IValueConverter
    {
        Dictionary<string, string> oeDetailTypes = new Dictionary<string, string>() { { "U", "Upper" }, { "L", "Lower" }, { "D", "Die" }, { "DS", "Die" }, { "R", "Reject" }, { "A", "Alignment" } };
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime start = DateTime.Now;
            var backgroundColor = SetLinearGradientBrush(Colors.Transparent, Colors.Transparent, Colors.Transparent, Colors.Transparent);

            try
            {
                if (value is EoiAllTabletProjectsView)
                {
                    try
                    {
                        EoiAllTabletProjectsView project = value as EoiAllTabletProjectsView;
                        bool priority = project.MarkedPriority is null ? false : project.MarkedPriority == "PRIORITY";
                        bool inactive = project.Complete == -1;
                        bool finished = project.Complete == 4;
                        bool onHold = project.HoldStatus == "On Hold";
                        bool submitted = project.Complete == 3;
                        bool drawn = project.Complete == 2;
                        bool started = project.Complete == 1;
                        bool sentBack = System.IO.File.Exists(@"\\engserver\workstations\TOOLING AUTOMATION\Project Specifications\" + project.ProjectNumber + "\\NEED_TO_FIX.txt");
                        if (inactive)
                        {
                            if (priority) { return SetLinearGradientBrushTablets(Colors.BlanchedAlmond, Colors.Transparent, Colors.Transparent, Colors.Red); }
                            return SetLinearGradientBrushTablets(Colors.BlanchedAlmond, Colors.Transparent, Colors.Transparent, Colors.Transparent);
                        }
                        if (onHold)
                        {
                            if (priority) { return SetLinearGradientBrushTablets(Colors.MediumPurple, Colors.Transparent, Colors.Transparent, Colors.Red); }
                            return SetLinearGradientBrushTablets(Colors.MediumPurple, Colors.Transparent, Colors.Transparent, Colors.Transparent);
                        }
                        if (finished)
                        {
                            if (priority) { return SetLinearGradientBrushTablets(Colors.GreenYellow, Colors.GreenYellow, Colors.GreenYellow, Colors.Red); }
                            return SetLinearGradientBrushTablets(Colors.GreenYellow, Colors.GreenYellow, Colors.GreenYellow, Colors.GreenYellow);
                        }
                        if (sentBack)
                        {
                            if (priority) { return SetLinearGradientBrushTablets(Colors.Orange, Colors.Transparent, Colors.Transparent, Colors.Red); }
                            return SetLinearGradientBrushTablets(Colors.Orange, Colors.Transparent, Colors.Transparent, Colors.Transparent);
                        }
                        if (submitted)
                        {
                            if (priority) { return SetLinearGradientBrushTablets(Colors.DodgerBlue, Colors.DodgerBlue, Colors.DodgerBlue, Colors.Red); }
                            return SetLinearGradientBrushTablets(Colors.DodgerBlue, Colors.DodgerBlue, Colors.DodgerBlue, Colors.Transparent);
                        }
                        if (drawn)
                        {
                            if (priority) { return SetLinearGradientBrushTablets(Colors.DodgerBlue, Colors.DodgerBlue, Colors.Transparent, Colors.Red); }
                            return SetLinearGradientBrushTablets(Colors.DodgerBlue, Colors.DodgerBlue, Colors.Transparent, Colors.Transparent);
                        }
                        if (started)
                        {
                            if (priority) { return SetLinearGradientBrushTablets(Colors.DodgerBlue, Colors.Transparent, Colors.Transparent, Colors.Red); }
                            return SetLinearGradientBrushTablets(Colors.DodgerBlue, Colors.Transparent, Colors.Transparent, Colors.Transparent);
                        }
                        if (priority) { return SetLinearGradientBrushTablets(Colors.Transparent, Colors.Transparent, Colors.Transparent, Colors.Red); }
                    }
                    catch (Exception ex)
                    {
                        
                    }
                }
                else if (value is EoiAllToolProjectsView)
                {
                    try
                    {
                        EoiAllToolProjectsView project = value as EoiAllToolProjectsView;
                        bool priority = project.MarkedPriority is null ? false : project.MarkedPriority == "PRIORITY";
                        bool inactive = project.Complete == -1;
                        bool finished = project.Complete == 5;
                        bool tablets = project.Complete == 1;
                        bool multitip = project.MultiTipSketch;
                        bool onHold = project.HoldStatus == "On Hold";
                        bool submitted = project.Complete == 4;
                        bool drawn = project.Complete == 3;
                        bool started = project.Complete == 2;
                        if (inactive)
                        {
                            if (priority) { return SetLinearGradientBrushTablets(Colors.BlanchedAlmond, Colors.Transparent, Colors.Transparent, Colors.Red); }
                            return SetLinearGradientBrushTablets(Colors.BlanchedAlmond, Colors.Transparent, Colors.Transparent, Colors.Transparent);
                        }
                        if (onHold)
                        {
                            if (priority) { return SetLinearGradientBrushTablets(Colors.MediumPurple, Colors.Transparent, Colors.Transparent, Colors.Red); }
                            return SetLinearGradientBrushTablets(Colors.MediumPurple, Colors.Transparent, Colors.Transparent, Colors.Transparent);
                        }
                        if (finished)
                        {
                            if (priority) { return SetLinearGradientBrushTablets(Colors.GreenYellow, Colors.GreenYellow, Colors.GreenYellow, Colors.Red); }
                            return SetLinearGradientBrushTablets(Colors.GreenYellow, Colors.GreenYellow, Colors.GreenYellow, Colors.GreenYellow);
                        }
                        if (submitted)
                        {
                            if (priority) { return SetLinearGradientBrushTablets(Colors.DodgerBlue, Colors.DodgerBlue, Colors.DodgerBlue, Colors.Red); }
                            return SetLinearGradientBrushTablets(Colors.DodgerBlue, Colors.DodgerBlue, Colors.DodgerBlue, Colors.Transparent);
                        }
                        if (drawn)
                        {
                            if (priority) { return SetLinearGradientBrushTablets(Colors.DodgerBlue, Colors.DodgerBlue, Colors.Transparent, Colors.Red); }
                            return SetLinearGradientBrushTablets(Colors.DodgerBlue, Colors.DodgerBlue, Colors.Transparent, Colors.Transparent);
                        }
                        if (started)
                        {
                            if (priority) { return SetLinearGradientBrushTablets(Colors.DodgerBlue, Colors.Transparent, Colors.Transparent, Colors.Red); }
                            return SetLinearGradientBrushTablets(Colors.DodgerBlue, Colors.Transparent, Colors.Transparent, Colors.Transparent);
                        }
                        if (tablets)
                        {
                            if (priority) { return SetLinearGradientBrushTablets(Colors.Yellow, Colors.Transparent, Colors.Transparent, Colors.Red); }
                            return SetLinearGradientBrushTablets(Colors.Yellow, Colors.Transparent, Colors.Transparent, Colors.Transparent);
                        }
                        if (multitip)
                        {
                            if (priority) { return SetLinearGradientBrushTablets(Colors.Gray, Colors.Transparent, Colors.Transparent, Colors.Red); }
                            return SetLinearGradientBrushTablets(Colors.Gray, Colors.Transparent, Colors.Transparent, Colors.Transparent);
                        }
                        if (priority) { return SetLinearGradientBrushTablets(Colors.Transparent, Colors.Transparent, Colors.Transparent, Colors.Red); }
                    }
                    catch (Exception ex)
                    {
                        
                    }
                }
                else if (value is EoiQuotesMarkedForConversionView)
                {
                    EoiQuotesMarkedForConversionView quote = value as EoiQuotesMarkedForConversionView;

                    if (quote.Rush == "Y") { return SetLinearGradientBrush(Colors.Transparent, Colors.Transparent, Colors.Transparent, Colors.Red); }
                    return SetLinearGradientBrush(Colors.Transparent, Colors.Transparent, Colors.Transparent, Colors.Transparent);
                }
                else if (value is EoiQuotesNotConvertedView)
                {
                    EoiQuotesNotConvertedView quote = value as EoiQuotesNotConvertedView;

                    if (quote.NeedsFollowUp == 1)
                    {
                        if ((value as EoiQuotesNotConvertedView).RushYorN == "Y") { return SetLinearGradientBrush(Colors.Pink, Colors.Transparent, Colors.Transparent, Colors.Red); }
                        return SetLinearGradientBrush(Colors.Pink, Colors.Transparent, Colors.Transparent, Colors.Transparent);
                    }
                    else if (quote.NeedsFollowUp == 2)
                    {
                        if ((value as EoiQuotesNotConvertedView).RushYorN == "Y") { return SetLinearGradientBrush(Colors.OrangeRed, Colors.Transparent, Colors.Transparent, Colors.Red); }
                        return SetLinearGradientBrush(Colors.OrangeRed, Colors.Transparent, Colors.Transparent, Colors.Transparent);
                    }

                    if ((value as EoiQuotesNotConvertedView).RushYorN == "Y") { return SetLinearGradientBrush(Colors.Transparent, Colors.Transparent, Colors.Transparent, Colors.Red); }
                    return SetLinearGradientBrush(Colors.Transparent, Colors.Transparent, Colors.Transparent, Colors.Transparent);
                }
                else if (parameter.ToString() == "NatoliOrderList")
                {
                    int daysToShip = ((value as NatoliOrderListFinal).ShipDate - DateTime.Now.Date).Days;

                    if (daysToShip < 0)
                    {
                        return SetLinearGradientBrush(Colors.Red, Colors.Red, Colors.Red, Colors.Transparent);
                    }
                    else if (daysToShip == 0)
                    {
                        return SetLinearGradientBrush(Colors.Orange, Colors.Orange, Colors.Orange, Colors.Transparent);
                    }
                    else if (daysToShip > 0 && daysToShip < 4)
                    {
                        return SetLinearGradientBrush(Colors.Yellow, Colors.Yellow, Colors.Yellow, Colors.Transparent);
                    }
                    else
                    {
                        return SetLinearGradientBrush(Colors.Transparent, Colors.Transparent, Colors.Transparent, Colors.Transparent);
                    }
                }
                else
                {
                    EoiAllOrdersView order = value as EoiAllOrdersView;
                    bool rush = order.RushYorN == "Y" || order.PaidRushFee == "Y";

                    if (order.BeingEntered == 1) { return backgroundColor; }

                    if (order.InTheOffice == 1)
                    {
                        if (order.DoNotProcess == 1)
                        {
                            if (rush) { return SetLinearGradientBrush(Colors.Pink, Colors.Transparent, Colors.Transparent, Colors.Red); }
                            return SetLinearGradientBrush(Colors.Pink, Colors.Transparent, Colors.Transparent, Colors.Transparent);
                        }
                        if (rush) { return SetLinearGradientBrush(Colors.Transparent, Colors.Transparent, Colors.Transparent, Colors.Red); }
                    }

                    if (order.EnteredUnscanned == 1)
                    {
                        bool running = order.Generating;
                        bool ran = !order.Generating && order.Generated;

                        if (order.DoNotProcess == 1)
                        {
                            if (rush) { return SetLinearGradientBrush(Colors.Pink, Colors.Transparent, Colors.Transparent, Colors.Red); }
                            return SetLinearGradientBrush(Colors.Pink, Colors.Transparent, Colors.Transparent, Colors.Transparent);
                        }
                        if (running)
                        {
                            if (rush) { return SetLinearGradientBrush(Colors.MediumPurple, Colors.Transparent, Colors.Transparent, Colors.Red); }
                            return SetLinearGradientBrush(Colors.MediumPurple, Colors.Transparent, Colors.Transparent, Colors.Transparent);
                        }
                        if (((order.ProcessState == "Failed" && order.ProcessState != "Complete") || order.TransitionName == "NeedInfo") && !ran)
                        {
                            if (rush) { return SetLinearGradientBrush(Colors.DarkGray, Colors.Transparent, Colors.Transparent, Colors.Red); }
                            return SetLinearGradientBrush(Colors.DarkGray, Colors.Transparent, Colors.Transparent, Colors.Transparent);
                        }
                        if (rush) { return SetLinearGradientBrush(Colors.Transparent, Colors.Transparent, Colors.Transparent, Colors.Red); }
                    }

                    if (order.InEngineering == 1)
                    {
                        if (order.DoNotProcess == 1)
                        {
                            if (rush) { return SetLinearGradientBrush(Colors.Pink, Colors.Transparent, Colors.Transparent, Colors.Red); }
                            return SetLinearGradientBrush(Colors.Pink, Colors.Transparent, Colors.Transparent, Colors.Transparent);
                        }
                        if (order.BeingChecked == 1)
                        {
                            if (rush) { return SetLinearGradientBrush(Colors.DodgerBlue, Colors.Transparent, Colors.Transparent, Colors.Red); }
                            return SetLinearGradientBrush(Colors.DodgerBlue, Colors.Transparent, Colors.Transparent, Colors.Transparent);
                        }
                        if (order.VariablesExist == 0)
                        {
                            if (rush) { return SetLinearGradientBrush(Colors.Orange, Colors.Transparent, Colors.Transparent, Colors.Red); }
                            return SetLinearGradientBrush(Colors.Orange, Colors.Transparent, Colors.Transparent, Colors.Transparent);
                        }
                        if (order.MarkedForChecking == 1)
                        {
                            if (rush) { return SetLinearGradientBrush(Colors.GreenYellow, Colors.Transparent, Colors.Transparent, Colors.Red); }
                            return SetLinearGradientBrush(Colors.GreenYellow, Colors.Transparent, Colors.Transparent, Colors.Transparent);
                        }
                        if (rush) { return SetLinearGradientBrush(Colors.Transparent, Colors.Transparent, Colors.Transparent, Colors.Red); }
                    }

                    if (order.ReadyToPrint == 1)
                    {
                        if (order.DoNotProcess == 1)
                        {
                            if (rush) { return SetLinearGradientBrush(Colors.Pink, Colors.Transparent, Colors.Transparent, Colors.Red); }
                            return SetLinearGradientBrush(Colors.Pink, Colors.Transparent, Colors.Transparent, Colors.Transparent);
                        }
                        if (order.VariablesExist == 0)
                        {
                            if (rush) { return SetLinearGradientBrush(Colors.Orange, Colors.Transparent, Colors.Transparent, Colors.Red); }
                            return SetLinearGradientBrush(Colors.Orange, Colors.Transparent, Colors.Transparent, Colors.Transparent);
                        }
                        else if (order.Tablet == 1 || order.Tool == 1 || order.Tm2 == 1)
                        {
                            bool tm2 = System.Convert.ToBoolean(order.Tm2);
                            bool tabletPrints = System.Convert.ToBoolean(order.Tablet);
                            bool toolPrints = System.Convert.ToBoolean(order.Tool);
                            List<string> hobNumbers = null;
                            hobNumbers = !string.IsNullOrEmpty(order.HobNumbers) && !string.IsNullOrEmpty(order.HobNumbers) ? order.HobNumbers.Split(",").ToList() : null;
                            if (tm2 || tabletPrints)
                            {
                                if (hobNumbers != null)
                                {
                                    foreach (string hobNumber in hobNumbers)
                                    {
                                        string path = @"\\engserver\workstations\tool_drawings\" + order.OrderNumber + @"\" + hobNumber + ".pdf";
                                        if (!System.IO.File.Exists(path))
                                        {
                                            goto Missing;
                                        }
                                    }
                                }
                            }

                            if (tm2 || toolPrints)
                            {
                                List<string> detailTypes = null;
                                detailTypes = !string.IsNullOrEmpty(order.DetailTypes) ? order.DetailTypes.Split(",").ToList() : null;
                                foreach (string detailTypeID in detailTypes)
                                {
                                    if (detailTypeID == "U" || detailTypeID == "L" || detailTypeID == "D" || detailTypeID == "DS" || detailTypeID == "R")
                                    {
                                        string detailType = oeDetailTypes[detailTypeID];
                                        detailType = detailType == "MISC" ? "REJECT" : detailType;
                                        string international = order.UnitOfMeasure;
                                        string path = @"\\engserver\workstations\tool_drawings\" + order.OrderNumber + @"\" + detailType + ".pdf";
                                        if (!System.IO.File.Exists(path))
                                        {
                                            goto Missing;
                                        }
                                        if (international == "M" && !System.IO.File.Exists(path.Replace(detailType, detailType + "_M")))
                                        {
                                            goto Missing;
                                        }
                                    }
                                }
                            }

                            goto NotMissing;

                        Missing:;
                            if (rush) { return SetLinearGradientBrush(Colors.MediumPurple, Colors.Transparent, Colors.Transparent, Colors.Red); }
                            return SetLinearGradientBrush(Colors.MediumPurple, Colors.Transparent, Colors.Transparent, Colors.Transparent);
                            goto Finished;

                        NotMissing:;
                            if (rush) { return SetLinearGradientBrush(Colors.Transparent, Colors.Transparent, Colors.Transparent, Colors.Red); }

                        Finished:;
                        }
                        if (rush) { return SetLinearGradientBrush(Colors.Transparent, Colors.Transparent, Colors.Transparent, Colors.Red); }
                    }

                    if (order.Printed == 1)
                    {
                        if (order.DoNotProcess == 1)
                        {
                            if (rush) { return SetLinearGradientBrush(Colors.Pink, Colors.Transparent, Colors.Transparent, Colors.Red); }
                            return SetLinearGradientBrush(Colors.Pink, Colors.Transparent, Colors.Transparent, Colors.Transparent);
                        }
                        if (order.VariablesExist == 0)
                        {
                            if (rush) { return SetLinearGradientBrush(Colors.Orange, Colors.Transparent, Colors.Transparent, Colors.Red); }
                            return SetLinearGradientBrush(Colors.Orange, Colors.Transparent, Colors.Transparent, Colors.Transparent);
                        }
                        else if (order.Tablet == 1 || order.Tool == 1 || order.Tm2 == 1)
                        {
                            bool tm2 = System.Convert.ToBoolean(order.Tm2);
                            bool tabletPrints = System.Convert.ToBoolean(order.Tablet);
                            bool toolPrints = System.Convert.ToBoolean(order.Tool);
                            List<OrderDetails> orderDetails;
                            OrderHeader orderHeader;
                            using var nat01context = new NAT01Context();
                            orderDetails = nat01context.OrderDetails.Where(o => o.OrderNo == order.OrderNumber * 100).ToList();
                            orderHeader = nat01context.OrderHeader.Single(o => o.OrderNo == order.OrderNumber * 100);
                            nat01context.Dispose();

                            if (tm2 || tabletPrints)
                            {
                                foreach (OrderDetails od in orderDetails)
                                {
                                    if (od.DetailTypeId.Trim() == "U" || od.DetailTypeId.Trim() == "L" || od.DetailTypeId.Trim() == "R")
                                    {
                                        string path = @"\\engserver\workstations\tool_drawings\" + order.OrderNumber + @"\" + od.HobNoShapeId.Trim() + ".pdf";
                                        if (!System.IO.File.Exists(path))
                                        {
                                            goto Missing;
                                        }
                                    }
                                }
                            }

                            if (tm2 || toolPrints)
                            {
                                foreach (OrderDetails od in orderDetails)
                                {
                                    if (od.DetailTypeId.Trim() == "U" || od.DetailTypeId.Trim() == "L" || od.DetailTypeId.Trim() == "D" || od.DetailTypeId.Trim() == "DS" || od.DetailTypeId.Trim() == "R")
                                    {
                                        string detailType = oeDetailTypes[od.DetailTypeId.Trim()];
                                        detailType = detailType == "MISC" ? "REJECT" : detailType;
                                        string international = orderHeader.UnitOfMeasure;
                                        string path = @"\\engserver\workstations\tool_drawings\" + order.OrderNumber + @"\" + detailType + ".pdf";
                                        if (!System.IO.File.Exists(path))
                                        {
                                            goto Missing;
                                        }
                                        if (international == "M" && !System.IO.File.Exists(path.Replace(detailType, detailType + "_M")))
                                        {
                                            goto Missing;
                                        }
                                    }
                                }
                            }

                            goto NotMissing;

                        Missing:;
                            if (rush) { return SetLinearGradientBrush(Colors.MediumPurple, Colors.Transparent, Colors.Transparent, Colors.Red); }
                            return SetLinearGradientBrush(Colors.MediumPurple, Colors.Transparent, Colors.Transparent, Colors.Transparent);
                            goto Finished;

                        NotMissing:;
                            if (rush) { return SetLinearGradientBrush(Colors.Transparent, Colors.Transparent, Colors.Transparent, Colors.Red); }

                        Finished:;
                        }
                        if (rush) { return SetLinearGradientBrush(Colors.Transparent, Colors.Transparent, Colors.Transparent, Colors.Red); }
                    }

                }
            }
            catch (Exception ex)
            {
                // System.Windows.MessageBox.Show(ex.Message);
            }

            return backgroundColor;
        }
        private static int GetNumberOfDays(string csr)
        {
            switch (csr)
            {
                case "Alex Heimberger":
                    return 14;
                case "Anna King":
                    return 7;
                case "Bryan Foy":
                    return 7;
                case "David Nelson":
                    return 7;
                case "Gregory Lyle":
                    return 14;
                case "Heather Lane":
                    return 7;
                case "Humberto Zamora":
                    return 14;
                case "James Willis":
                    return 14;
                case "Miral Bouzitoun":
                    return 14;
                case "Nicholas Tarte":
                    return 14;
                case "Samantha Bowman":
                    return 7;
                case "Tiffany Simonpietri":
                    return 7;
                default:
                    return 14;
            }
        }
        private LinearGradientBrush SetLinearGradientBrush(Color first, Color second, Color third, Color fourth)
        {
            LinearGradientBrush linearGradientBrush;
            GradientStop gradientStop_0 = new GradientStop(first, -0.05);
            GradientStop gradientStop_1 = new GradientStop(second, 0.3);
            GradientStop gradientStop_2 = new GradientStop(third, 0.8);
            GradientStop gradientStop_3 = new GradientStop(fourth, 1.05);

            GradientStopCollection gradientStops = new GradientStopCollection();
            gradientStops.Add(gradientStop_0);
            gradientStops.Add(gradientStop_1);
            gradientStops.Add(gradientStop_2);
            gradientStops.Add(gradientStop_3);
            linearGradientBrush = new LinearGradientBrush(gradientStops, new System.Windows.Point(0.0, 0.0), new System.Windows.Point(1.0, 1.0));
            linearGradientBrush.Opacity = 1.0;
            return linearGradientBrush;
        }

        private LinearGradientBrush SetLinearGradientBrushTablets(Color first, Color second, Color third, Color fourth)
        {
            LinearGradientBrush linearGradientBrush;
            GradientStop gradientStop_0 = new GradientStop(first, 0.25);
            GradientStop gradientStop_1 = new GradientStop(second, 0.5);
            GradientStop gradientStop_2 = new GradientStop(third, 0.75);
            GradientStop gradientStop_2_1 = new GradientStop(Colors.Transparent, 0.9);
            GradientStop gradientStop_2_2 = new GradientStop(fourth, 1.0);
            GradientStop gradientStop_3 = new GradientStop(fourth, 1.1);

            GradientStopCollection gradientStops = new GradientStopCollection();
            gradientStops.Add(gradientStop_0);
            gradientStops.Add(gradientStop_1);
            gradientStops.Add(gradientStop_2);
            gradientStops.Add(gradientStop_2_1);
            gradientStops.Add(gradientStop_2_2);
            gradientStops.Add(gradientStop_3);
            linearGradientBrush = new LinearGradientBrush(gradientStops, new System.Windows.Point(0.0, 0.0), new System.Windows.Point(1.0, 0.0));
            linearGradientBrush.Opacity = 0.7;
            return linearGradientBrush;
        }

        //private LinearGradientBrush SetLinearGradientBrushTools(Color first, Color second, Color third)
        //{
        //    LinearGradientBrush linearGradientBrush;
        //    GradientStop gradientStop_0 = new GradientStop(first, 0.33333);
        //    GradientStop gradientStop_1 = new GradientStop(second, 0.66666);
        //    GradientStop gradientStop_1_1 = new GradientStop(Colors.Transparent, 0.8);
        //    GradientStop gradientStop_1_2 = new GradientStop(third, 1.0);
        //    GradientStop gradientStop_3 = new GradientStop(third, 1.1);

        //    GradientStopCollection gradientStops = new GradientStopCollection();
        //    gradientStops.Add(gradientStop_0);
        //    gradientStops.Add(gradientStop_1);
        //    gradientStops.Add(gradientStop_1_1);
        //    gradientStops.Add(gradientStop_1_2);
        //    gradientStops.Add(gradientStop_3);
        //    linearGradientBrush = new LinearGradientBrush(gradientStops, new System.Windows.Point(0.0, 0.0), new System.Windows.Point(1.0, 0.0));
        //    linearGradientBrush.Opacity = 0.7;
        //    return linearGradientBrush;
        //}

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;

            throw new NotImplementedException();
        }
    }
}
