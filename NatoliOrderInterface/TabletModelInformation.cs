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
        public string HobNumber { get; }
        public string DieNumber { get; set; }
        public double Width { get; set; }
        public double Length { get; set; }
        public double EndRadius { get; set; }
        public double SideRadius { get; set; }
        public double CupDepth { get; set; }
        public double CupRadius { get; set; }
        public double Land { get; set; }
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
        private string[] sideRadiusDims = new string[]
        {
            "SideRadius@TabletOvalSketch"
        };
        private string[] cupDepthDims = new string[]
        {
            "CupDepth@TopConcaveCupSketch",
            "CupDepth@MinorConcaveCupSketch (00X10)",
            "Cup Depth@Sketch4"
        };
        private string[] cupRadiusDims = new string[]
        {
            "CupRadius@TopConcaveCupSketch",
            "CupRadius@MinorConcaveCupSketch (00X10)"
        };
        private string[] landDims = new string[]
        {
            "Land@Thickness&Land Callouts",
            "Land@MinorLand Callout",
            "Land@Sketch3"
        };
        SldWorks sldWorks = new SldWorks();
        public TabletModelInformation()
        {
            string hobNumber = ((ModelDoc2)sldWorks.ActiveDoc).GetType() == (int)swDocumentTypes_e.swDocPART ?
                ((ModelDoc2)sldWorks.ActiveDoc).GetTitle() :
                ((ModelDoc2)sldWorks.ActiveDoc).GetTitle().Remove(((ModelDoc2)sldWorks.ActiveDoc).GetTitle().IndexOf(' '));
            HobNumber = hobNumber;
            string modelPath = GetTabletPath(hobNumber) + ".SLDPRT";
            ModelDoc2 tablet = (ModelDoc2)sldWorks.OpenDoc(modelPath, (int)swDocumentTypes_e.swDocPART);
            if (tablet is null)
                tablet = (ModelDoc2)sldWorks.ActivateDoc(hobNumber);
            CustomPropertyManager cpmTablet = (CustomPropertyManager)tablet.Extension.CustomPropertyManager[""];
            GetDieNumber(tablet, cpmTablet);
            GetSize(tablet);
            sldWorks.CloseDoc(HobNumber);
        }
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
            SideRadius = GetDimValue(tablet, sideRadiusDims);
            CupDepth = GetDimValue(tablet, cupDepthDims);
            Land = GetDimValue(tablet, landDims);
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
