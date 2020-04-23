using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using NatoliOrderInterface.Models;
using System.Linq;

namespace NatoliOrderInterface
{
    class ToolTipConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is EoiQuotesMarkedForConversionView)
                {
                    return (value as EoiQuotesMarkedForConversionView).ShipDate.Trim();
                }
                else if (value is EoiAllTabletProjectsView)
                {
                    using var __nat02context = new NAT02Context();
                    if (__nat02context.EoiProjectsOnHold.Any(p => p.ProjectNumber == (value as EoiAllTabletProjectsView).ProjectNumber && p.RevisionNumber == (value as EoiAllTabletProjectsView).RevisionNumber))
                    {
                        string comment = string.IsNullOrEmpty(__nat02context.EoiProjectsOnHold.FirstOrDefault(p => p.ProjectNumber == (value as EoiAllTabletProjectsView).ProjectNumber && p.RevisionNumber == (value as EoiAllTabletProjectsView).RevisionNumber).OnHoldComment.Trim()) ?
                            "No Comment" :
                            __nat02context.EoiProjectsOnHold.First(p => p.ProjectNumber == (value as EoiAllTabletProjectsView).ProjectNumber && p.RevisionNumber == (value as EoiAllTabletProjectsView).RevisionNumber).OnHoldComment.Trim();
                        __nat02context.Dispose();
                        return comment;
                    }
                    else
                    {
                        __nat02context.Dispose();
                        return null;
                    }
                }
                else if (value is EoiAllToolProjectsView)
                {
                    using var __nat02context = new NAT02Context();
                    if (__nat02context.EoiProjectsOnHold.Any(p => p.ProjectNumber == (value as EoiAllToolProjectsView).ProjectNumber && p.RevisionNumber == (value as EoiAllToolProjectsView).RevisionNumber))
                    {
                        string comment = string.IsNullOrEmpty(__nat02context.EoiProjectsOnHold.FirstOrDefault(p => p.ProjectNumber == (value as EoiAllToolProjectsView).ProjectNumber && p.RevisionNumber == (value as EoiAllToolProjectsView).RevisionNumber).OnHoldComment.Trim()) ?
                            "No Comment" :
                            __nat02context.EoiProjectsOnHold.First(p => p.ProjectNumber == (value as EoiAllToolProjectsView).ProjectNumber && p.RevisionNumber == (value as EoiAllToolProjectsView).RevisionNumber).OnHoldComment.Trim();
                        __nat02context.Dispose();
                        return comment;
                    }
                    else
                    {
                        __nat02context.Dispose();
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            catch( Exception ex)
            {
                IMethods.WriteToErrorLog("ToolTipConverter => Convert()", ex.Message, new User());
                return null;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;

            throw new NotImplementedException();
        }
    }
}
