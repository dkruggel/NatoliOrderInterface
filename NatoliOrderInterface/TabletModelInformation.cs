using System;
using System.Collections.Generic;
using System.Text;
using DocumentFormat.OpenXml.Drawing;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace NatoliOrderInterface
{
    public class TabletModelInformation
    {
        private string HobNumber { get; set; }
        public string DieNumber { get; set; }
        public double Width { get; set; }
        public double Length { get; set; }
        public double EndRadius { get; set; }
        private string[] widthDims = new string[]
        {
            "TabletWidth@TabletDiameterSketch",
            "TabletWidth@TabletCapsuleSketch",
            "TabletWidth@TabletOvalSketch",
            "Diameter@Sketch2",
            "Diameter@Sketch1"
        };
        private string[] lengthDims = new string[]
        {
            "TabletLength@TabletCapsuleSketch",
            "TabletLength@TabletOvalSketch"
        };
        private string[] endRadiusDims = new string[]
        {
            "EndRadius@TabletCapsuleSketch",
            "EndRadius@TabletOvalSketch"
        };
        SldWorks sldWorks = new SldWorks();
        public TabletModelInformation(string hobNumber)
        {
            HobNumber = hobNumber;
            string modelPath = GetTabletPath(hobNumber) + ".SLDPRT";
            ModelDoc2 tablet = (ModelDoc2)sldWorks.OpenDoc(modelPath, (int)swDocumentTypes_e.swDocPART);
            CustomPropertyManager cpmTablet = (CustomPropertyManager)tablet.Extension.CustomPropertyManager[""];
            GetDieNumber(tablet, cpmTablet);
            GetSize(tablet);
            sldWorks.CloseDoc(HobNumber);
        }
        private string GetTabletPath(string hobNumber)
        {
            string path = @"\\engserver\workstations\tablets\NAT";
            int i = 6;

            try
            {
                while (hobNumber[0] == '0')
                {
                    hobNumber = hobNumber.Remove(0, 1);
                    i--;
                }

                switch (hobNumber.Length)
                {
                    case 6:
                        path += hobNumber[0..2] + "X\\NAT" + hobNumber[0..3] + "\\" + hobNumber;
                        break;
                    case 5:
                        path += hobNumber[0..1] + "X\\NAT" + hobNumber[0..2] + "\\" + hobNumber;
                        break;
                    case 4:
                        path += "0X\\NAT0" + hobNumber[0..1] + "\\" + hobNumber;
                        break;
                    default:
                        path += "0X\\NAT00\\" + hobNumber;
                        break;
                }

                return path;
            }
            catch (Exception ex)
            {
                return "";
            }
            return "";
        }
        private void GetDieNumber(ModelDoc2 tablet, CustomPropertyManager cpmTablet)
        {
            DieNumber = cpmTablet.Get("Die_no");
        }
        private void GetSize(ModelDoc2 tablet)
        {
            Width = GetDimValue(tablet, widthDims);
            Length = GetDimValue(tablet, lengthDims);
            EndRadius = GetDimValue(tablet, endRadiusDims);
        }
        private double GetDimValue(ModelDoc2 tablet, string[] dims)
        {
            bool bret = false;
            foreach (string dim in dims)
            {
                bret = tablet.Extension.SelectByID2(dim, "DIMENSION", 0, 0, 0, false, 0, null, 0);
                if (bret)
                {
                    return ((Dimension)tablet.Parameter(dim)).SystemValue / 0.0254;
                }
            }
            return 0.0;
        }
    }
}
