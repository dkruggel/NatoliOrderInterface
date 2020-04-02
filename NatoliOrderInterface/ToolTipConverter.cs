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
            if (value is EoiQuotesMarkedForConversionView)
            {
                return (value as EoiQuotesMarkedForConversionView).ShipDate.Trim();
            }
            else if (value is EoiAllTabletProjectsView)
            {
                using var __nat02context = new NAT02Context();
                if (__nat02context.EoiProjectsOnHold.Any(p => p.ProjectNumber == (value as EoiAllTabletProjectsView).ProjectNumber && p.RevisionNumber == (value as EoiAllTabletProjectsView).RevisionNumber))
                {
                    return string.IsNullOrEmpty(__nat02context.EoiProjectsOnHold.FirstOrDefault(p => p.ProjectNumber == (value as EoiAllTabletProjectsView).ProjectNumber && p.RevisionNumber == (value as EoiAllTabletProjectsView).RevisionNumber).OnHoldComment.Trim()) ?
                        "No Comment" :
                        __nat02context.EoiProjectsOnHold.First(p => p.ProjectNumber == (value as EoiAllTabletProjectsView).ProjectNumber && p.RevisionNumber == (value as EoiAllTabletProjectsView).RevisionNumber).OnHoldComment.Trim();
                }
                else
                {
                    return null;
                }
                __nat02context.Dispose();
            }
            else if (value is EoiAllToolProjectsView)
            {
                using var __nat02context = new NAT02Context();
                if (__nat02context.EoiProjectsOnHold.Any(p => p.ProjectNumber == (value as EoiAllToolProjectsView).ProjectNumber && p.RevisionNumber == (value as EoiAllToolProjectsView).RevisionNumber))
                {
                    return string.IsNullOrEmpty(__nat02context.EoiProjectsOnHold.FirstOrDefault(p => p.ProjectNumber == (value as EoiAllToolProjectsView).ProjectNumber && p.RevisionNumber == (value as EoiAllToolProjectsView).RevisionNumber).OnHoldComment.Trim()) ?
                        "No Comment" :
                        __nat02context.EoiProjectsOnHold.First(p => p.ProjectNumber == (value as EoiAllToolProjectsView).ProjectNumber && p.RevisionNumber == (value as EoiAllToolProjectsView).RevisionNumber).OnHoldComment.Trim();
                }
                else
                {
                    return null;
                }
                __nat02context.Dispose();
            }
            else
            {
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
