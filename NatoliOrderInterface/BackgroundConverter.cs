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

namespace NatoliOrderInterface
{
    class BackgroundConverter : IValueConverter
    {
        Dictionary<string, string> oeDetailTypes = new Dictionary<string, string>() { { "U", "Upper" }, { "L", "Lower" }, { "D", "Die" }, { "DS", "Die" }, { "R", "Reject" }, { "A", "Alignment" } };
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double orderNumber = double.Parse(value.ToString());

            using var _nat02Context = new NAT02Context();
            using var nat01context = new NAT01Context();
            using var _natBCContext = new NATBCContext();

            try
            {
                if (parameter.ToString() == "Orders")
                {
                        EoiAllOrdersView order = _nat02Context.EoiAllOrdersView.Single(o => o.OrderNumber == orderNumber);
                        bool rush = order.RushYorN == "Y" || order.PaidRushFee == "Y";

                        if (order.BeingEntered == 1) { return new SolidColorBrush(Colors.Transparent); }

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
                            if (order.DoNotProcess == 1)
                            {
                                if (rush) { return SetLinearGradientBrush(Colors.Pink, Colors.Transparent, Colors.Transparent, Colors.Red); }
                                return SetLinearGradientBrush(Colors.Pink, Colors.Transparent, Colors.Transparent, Colors.Transparent);
                            }
                            if ((order.ProcessState == "Failed" && order.ProcessState != "Complete") || order.TransitionName == "NeedInfo")
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
                            else if (order.Tablet == 1 || order.Tool == 1 || order.Tm2 == 1)
                            {
                                bool tm2 = System.Convert.ToBoolean(order.Tm2);
                                bool tabletPrints = System.Convert.ToBoolean(order.Tablet);
                                bool toolPrints = System.Convert.ToBoolean(order.Tool);
                                List<OrderDetails> orderDetails;
                                OrderHeader orderHeader;
                                orderDetails = nat01context.OrderDetails.Where(o => o.OrderNo == order.OrderNumber * 100).ToList();
                                orderHeader = nat01context.OrderHeader.Single(o => o.OrderNo == order.OrderNumber * 100);

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


                    if (order.Printed == 1)
                    {
                            if (order.DoNotProcess == 1)
                            {
                                if (rush) { return SetLinearGradientBrush(Colors.Pink, Colors.Transparent, Colors.Transparent, Colors.Red); }
                                return SetLinearGradientBrush(Colors.Pink, Colors.Transparent, Colors.Transparent, Colors.Transparent);
                            }
                            else if (order.Tablet == 1 || order.Tool == 1 || order.Tm2 == 1)
                            {
                                bool tm2 = System.Convert.ToBoolean(order.Tm2);
                                bool tabletPrints = System.Convert.ToBoolean(order.Tablet);
                                bool toolPrints = System.Convert.ToBoolean(order.Tool);
                                List<OrderDetails> orderDetails;
                                OrderHeader orderHeader;
                                orderDetails = nat01context.OrderDetails.Where(o => o.OrderNo == order.OrderNumber * 100).ToList();
                                orderHeader = nat01context.OrderHeader.Single(o => o.OrderNo == order.OrderNumber * 100);

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
                else if (parameter.ToString() == "Tablets")
                {
                    try
                    {
                        EoiAllTabletProjectsView project = _nat02Context.EoiAllTabletProjectsView.Single(p => p.ProjectNumber == orderNumber);
                        bool priority = project.MarkedPriority is null ? false : project.MarkedPriority == "PRIORITY";
                        bool finished = _nat02Context.EoiProjectsFinished.Where(p => p.ProjectNumber == project.ProjectNumber && p.RevisionNumber == project.RevisionNumber).Any();
                        bool onHold = project.HoldStatus == "On Hold";
                        bool submitted = project.TabletSubmittedBy is null ? false : project.TabletSubmittedBy.Length > 0;
                        bool drawn = project.TabletDrawnBy.Length > 0;
                        bool started = project.ProjectStartedTablet.Length > 0;

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
                        if (priority) { return SetLinearGradientBrushTablets(Colors.Transparent, Colors.Transparent, Colors.Transparent, Colors.Red); }
                    }
                    catch (Exception ex)
                    {
                        
                    }
                }
                else if (parameter.ToString() == "Tools")
                {
                    try
                    {
                        EoiAllToolProjectsView project = _nat02Context.EoiAllToolProjectsView.Single(p => p.ProjectNumber == orderNumber);
                        bool priority = project.MarkedPriority is null ? false : project.MarkedPriority == "PRIORITY";
                        bool finished = _nat02Context.EoiProjectsFinished.Where(p => p.ProjectNumber == project.ProjectNumber && p.RevisionNumber == project.RevisionNumber).Any();
                        using var projectsContext = new ProjectsContext();
                        bool tablets = (bool)projectsContext.ProjectSpecSheet.First(p => p.ProjectNumber == project.ProjectNumber && p.RevisionNumber == project.RevisionNumber).Tablet &&
                                       string.IsNullOrEmpty(projectsContext.ProjectSpecSheet.First(p => p.ProjectNumber == project.ProjectNumber && p.RevisionNumber == project.RevisionNumber).TabletCheckedBy);
                        bool multitip = (bool)projectsContext.ProjectSpecSheet.First(p => p.ProjectNumber == project.ProjectNumber && p.RevisionNumber == project.RevisionNumber).MultiTipSketch;
                        projectsContext.Dispose();
                        bool onHold = project.HoldStatus == "On Hold";
                        bool drawn = project.ToolDrawnBy.Length > 0;
                        bool started = project.ProjectStartedTool.Length > 0;

                        if (onHold)
                        {
                            if (priority) { return SetLinearGradientBrushTools(Colors.MediumPurple, Colors.Transparent, Colors.Red); }
                            return SetLinearGradientBrushTools(Colors.MediumPurple, Colors.Transparent, Colors.Transparent);
                        }
                        if (finished)
                        {
                            if (priority) { return SetLinearGradientBrushTools(Colors.GreenYellow, Colors.GreenYellow, Colors.Red); }
                            return SetLinearGradientBrushTools(Colors.GreenYellow, Colors.GreenYellow, Colors.GreenYellow);
                        }
                        if (drawn)
                        {
                            if (priority) { return SetLinearGradientBrushTools(Colors.DodgerBlue, Colors.DodgerBlue, Colors.Red); }
                            return SetLinearGradientBrushTools(Colors.DodgerBlue, Colors.DodgerBlue, Colors.Transparent);
                        }
                        if (started)
                        {
                            if (priority) { return SetLinearGradientBrushTools(Colors.DodgerBlue, Colors.Transparent, Colors.Red); }
                            return SetLinearGradientBrushTools(Colors.DodgerBlue, Colors.Transparent, Colors.Transparent);
                        }
                        if (tablets)
                        {
                            if (priority) { return SetLinearGradientBrushTools(Colors.Yellow, Colors.Transparent, Colors.Red); }
                            return SetLinearGradientBrushTools(Colors.Yellow, Colors.Transparent, Colors.Transparent);
                        }
                        if (multitip)
                        {
                            if (priority) { return SetLinearGradientBrushTools(Colors.Gray, Colors.Transparent, Colors.Red); }
                            return SetLinearGradientBrushTools(Colors.Gray, Colors.Transparent, Colors.Transparent);
                        }
                        if (priority) { return SetLinearGradientBrushTools(Colors.Transparent, Colors.Transparent, Colors.Red); }
                    }
                    catch (Exception ex)
                    {
                        
                    }
                }
                else if (parameter.ToString() == "Quotes")
                {

                }
                else if (parameter.ToString() == "NatoliOrderList")
                {
                    OrderHeader order = nat01context.OrderHeader.Single(o => o.OrderNo == orderNumber * 100);
                    DateTime date = ((DateTime)order.RequestedShipDateRev).Date.ToShortDateString() == "1/1/1901" ? ((DateTime)order.RequestedShipDate).Date : ((DateTime)order.RequestedShipDateRev).Date;
                    int daysToShip = (date.Date - DateTime.Now.Date).Days;

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
            }
            catch (Exception ex)
            {
                // System.Windows.MessageBox.Show(ex.Message);
            }

            _natBCContext.Dispose();
            nat01context.Dispose();
            _nat02Context.Dispose();

            return new SolidColorBrush(Colors.Transparent);
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

        private LinearGradientBrush SetLinearGradientBrushTools(Color first, Color second, Color third)
        {
            LinearGradientBrush linearGradientBrush;
            GradientStop gradientStop_0 = new GradientStop(first, 0.33333);
            GradientStop gradientStop_1 = new GradientStop(second, 0.66666);
            GradientStop gradientStop_1_1 = new GradientStop(Colors.Transparent, 0.8);
            GradientStop gradientStop_1_2 = new GradientStop(third, 1.0);
            GradientStop gradientStop_3 = new GradientStop(third, 1.1);

            GradientStopCollection gradientStops = new GradientStopCollection();
            gradientStops.Add(gradientStop_0);
            gradientStops.Add(gradientStop_1);
            gradientStops.Add(gradientStop_1_1);
            gradientStops.Add(gradientStop_1_2);
            gradientStops.Add(gradientStop_3);
            linearGradientBrush = new LinearGradientBrush(gradientStops, new System.Windows.Point(0.0, 0.0), new System.Windows.Point(1.0, 0.0));
            linearGradientBrush.Opacity = 0.7;
            return linearGradientBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;

            throw new NotImplementedException();
        }
    }
}
