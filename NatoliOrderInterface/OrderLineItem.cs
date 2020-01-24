using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using NatoliOrderInterface.Models;
using NatoliOrderInterface.Models.NAT01;
using NatoliOrderInterface.Models.NEC;
using System.Linq;

namespace NatoliOrderInterface
{
    public class OrderLineItem
    {
        #region Declarations and Get Sets
        private WorkOrder order;
        public WorkOrder Order
        {
            get { return order; }
            set { this.order = value; }
        }

        private short lineItemNumber;
        public short LineItemNumber
        {
            get { return lineItemNumber; }
            set { this.lineItemNumber = value; }
        }

        private int qty;
        public int QTY
        {
            get { return qty; }
            set { this.qty = value; }
        }

        private string lineItemType;
        public string LineItemType
        {
            get { return lineItemType; }
            set { this.lineItemType = value; }
        }

        private string hobNoShapeID;
        public string HobNoShapeID
        {
            get { return hobNoShapeID; }
            set { this.hobNoShapeID = value; }
        }

        private short machineNo;
        public short MachineNo
        {
            get { return machineNo; }
            set { this.machineNo = value; }
        }

        private string material;
        public string Material
        {
            get { return material; }
            set { this.material = value; }
        }

        private string stockSize;
        public string StockSize
        {
            get { return stockSize; }
            set { this.stockSize = value; }
        }

        private string hobDescription1;
        public string HobDescription1
        {
            get { return hobDescription1; }
            set { this.hobDescription1 = value; }
        }

        private string hobDescription2;
        public string HobDescription2
        {
            get { return hobDescription2; }
            set { this.hobDescription2 = value; }
        }

        private string hobDescription3;
        public string HobDescription3
        {
            get { return hobDescription3; }
            set { this.hobDescription3 = value; }
        }

        private string shape;
        public string Shape
        {
            get { return shape; }
            set { this.shape = value; }
        }

        private string machineDescription;
        public string MachineDescription
        {
            get { return machineDescription; }
            set { this.machineDescription = value; }
        }

        private string hobStatus;
        public string HobStatus
        {
            get { return hobStatus; }
            set { this.hobStatus = value; }
        }

        private string hobYorNorD;
        public string HobYorNorD
        {
            get { return hobYorNorD; }
            set { this.hobYorNorD = value; }
        }

        private short tipQTY;
        public short TipQTY
        {
            get { return tipQTY; }
            set { this.tipQTY = value; }
        }

        private string boreCircle;
        public string BoreCircle
        {
            get { return boreCircle; }
            set { this.boreCircle = value; }
        }

        private Dictionary<string, SolidColorBrush> sheetColorDict = new Dictionary<string, SolidColorBrush> {
            { "G", new SolidColorBrush(Colors.DarkSeaGreen) },
            { "W", new SolidColorBrush(Colors.White) },
            { "Y", new SolidColorBrush(Colors.Khaki) },
            { "H", new SolidColorBrush(Colors.White) },
            { "", new SolidColorBrush(Colors.White) }};

        private SolidColorBrush sheetColor;
        public SolidColorBrush SheetColor
        {
            get { return sheetColor; }
            set { this.sheetColor = value; }
        }

        private string title;
        public string Title
        {
            get { return title; }
            set { this.title = value; }
        }

        private List<string> optionNumbers = new List<string>();
        public List<string> OptionNumbers { get => optionNumbers; set => optionNumbers = value; }
        public Dictionary<short, string[]> Options { get => options; set => options = value; }

        private Dictionary<short, string[]> options = new Dictionary<short, string[]>();


        private readonly Dictionary<string, string> titles = new Dictionary<string, string> {
            { "A","ALIGNMENT TOOL" },
            { "CT","COPPER TABLETS" },
            { "D","DIE" },
            { "DH","DIE HOLDER" },
            { "DI","DIE INSERT" },
            { "DP","DIE PLATE" },
            { "DS","DIE SEGMENT" },
            { "E","ELEC. DOCS" },
            { "H","HOB" },
            { "K","KEY" },
            { "L","LOWER" },
            { "LA","LOWER ASSEMBLY" },
            { "LC","LOWER CAP" },
            { "LCR","CORE ROD" },
            { "LCRK","CORE ROD KEY" },
            { "LCRKC","CORE ROD KEY COLLAR" },
            { "LCRP","CORE ROD PUNCH" },
            { "LH","LOWER HOLDER" },
            { "LHD","LOWER HEAD" },
            { "LT","LOWER TIP" },
            { "M","MISC." },
            { "MC","MISC. CHARGE" },
            { "R","REJECT" },
            { "RA","REJECT ASSEMBLY" },
            { "RC","REJECT CAP" },
            { "RET","RETURN SAMPLES" },
            { "RH","REJECT HOLDER" },
            { "RHD","REJECT HEAD" },
            { "RT","REJECT TIP" },
            { "T","TOOLING BOX" },
            { "TM","TM-II DATA" },
            { "U","UPPER" },
            { "UA","UPPER ASSEMBLY" },
            { "UC","UPPER CAP" },
            { "UH","UPPER HOLDER" },
            { "UHD","UPPER HEAD" },
            { "UT","UPPER TIP" },
            { "Z","PHYS. DOCS" },
        };

        #endregion
        private OrderDetails orderDetails = new OrderDetails();
        private List<OrderDetailOptions> orderDetailOptions = new List<OrderDetailOptions>();
        private MachineList machineList = new MachineList();
        private HobList hobList = new HobList();
        private SteelType steelType = new SteelType();
        private List<OrdOptionValueASingleNum> optionValuesA;
        private List<OrdOptionValueBDoubleNum> optionValuesB;
        private List<OrdOptionValueCTolerance> optionValuesC;
        private List<OrdOptionValueDDegreeVal> optionValuesD;
        private List<OrdOptionValueESmallText> optionValuesE;
        private List<OrdOptionValueFLargeText> optionValuesF;
        private List<OrdOptionValueGDegrees> optionValuesG;
        private List<OrdOptionValueHHardness> optionValuesH;
        private List<OrdOptionValueIHardness2> optionValuesI;
        private List<OrdOptionValueJOptionMult> optionValuesJ;
        private List<OrdOptionValueKVendor> optionValuesK;
        private List<OrdOptionValueLSurfaceTreat> optionValuesL;
        private List<OrdOptionValueMScrew> optionValuesM;
        private List<OrdOptionValueNColor> optionValuesN;
        private List<OrdOptionValueOInteger> optionValuesO;
        private List<OrdOptionValuePDegDec> optionValuesP;
        private List<OrdOptionValueQDimensions> optionValuesQ;
        private List<OrdOptionValueRIntegerText> optionValuesR;
        private List<OrdOptionValueSText> optionValuesS;
        private List<OrdOptionValueTDecText> optionValuesT;
        private List<OptionsList> optionsList = new List<OptionsList>();



        public OrderLineItem(WorkOrder workOrder, short lineNumber = 1)
        {
            try
            {
                this.order = workOrder;
                this.lineItemNumber = lineNumber;
                using (NAT01Context nat01Context = new NAT01Context())
                {
                    orderDetails = nat01Context.OrderDetails.FirstOrDefault(x => x.OrderNo == order.OrderNumber * 100 && x.LineNumber == lineItemNumber);
                    lineItemType = orderDetails.DetailTypeId.ToString().Trim();
                    orderDetailOptions = nat01Context.OrderDetailOptions.Where(x => x.OrderNumber == order.OrderNumber * 100 && x.OrderDetailLineNo == lineItemNumber && !string.IsNullOrEmpty(x.OptionCode.Trim())).ToList();
                    machineNo = orderDetails.MachineNo.GetValueOrDefault();
                    machineList = nat01Context.MachineList.Where(m => m.MachineNo == machineNo).FirstOrDefault();
                    steelType = nat01Context.SteelType.Where(s => s.TypeId == orderDetails.SteelId).FirstOrDefault();
                    optionsList = nat01Context.OptionsList.ToList();
                    nat01Context.Dispose();
                    if (MachineNo != 0 && (lineItemType == "U" || lineItemType == "L" || lineItemType == "R" || lineItemType == "UH" || lineItemType == "LH" || lineItemType == "RH") && (machineList.MachineTypePrCode.Trim() == "B" || machineList.MachineTypePrCode.Trim() == "D"))
                    {
                        order.CanRunOnAutocell = true;
                    }
                    SetInfo(orderDetails, orderDetailOptions, optionsList);
                }
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("WorkOrder.cs -> OrderNumber: " + order.OrderNumber + "LineNumber: " + lineNumber, ex.Message, null);
            }
        }
        public void SetInfo(OrderDetails orderDetails, List<OrderDetailOptions> orderDetailOptions, List<OptionsList> optionsList)
        {
            hobNoShapeID = orderDetails.HobNoShapeId;
            #region HobYorNorD
            if ((lineItemType == "U" || lineItemType == "UT" || lineItemType == "L" || lineItemType == "LT" || lineItemType == "R" || lineItemType == "RT") && hobNoShapeID.Length == 6)
            {
                NAT01Context nat01Context = new NAT01Context();
                if (nat01Context.HobList.Any(h => h.HobNo == orderDetails.HobNoShapeId && h.TipQty == (orderDetails.TipQty ?? 1) && h.BoreCircle == (orderDetails.BoreCircle ?? 0)))
                {
                    hobList = nat01Context.HobList.First(h => h.HobNo == orderDetails.HobNoShapeId && h.TipQty == (orderDetails.TipQty ?? 1) && h.BoreCircle == (orderDetails.BoreCircle ?? 0));
                    hobYorNorD = string.IsNullOrEmpty(hobList.HobYorNorD) ? "" : hobList.HobYorNorD;
                }
                else
                {
                    hobYorNorD = "";
                }
                nat01Context.Dispose();
            }
            else
            {
                hobYorNorD = "";
            }
            #endregion
            #region SheetColor
            try
            {
                sheetColor = sheetColorDict[orderDetails.SheetColor.Trim()];
            }
            catch
            {
                sheetColor = new SolidColorBrush(Colors.White);
            }
            #endregion
            qty = orderDetails.QuantityOrdered;
            title = titles[lineItemType];
            tipQTY = orderDetails.TipQty is null ? (short)1 : (short)orderDetails.TipQty;
            boreCircle = orderDetails.BoreCircle == null ? "" : Convert.ToDouble(orderDetails.BoreCircle) == 0 ? "" : String.Format("{0:0.0000}", orderDetails.BoreCircle);
            stockSize = lineItemType == "U" ? machineList.UpperSize.Trim() : lineItemType == "L" ? machineList.LowerSize.Trim() : "";
            hobDescription1 = orderDetails.Desc1.ToString().Trim();
            hobDescription2 = String.Join(" ", orderDetails.Desc3.Trim(), orderDetails.Desc4.Trim()).Trim();
            HobDescription3 = String.Join(" ", orderDetails.Desc5.Trim(), orderDetails.Desc6.Trim(), orderDetails.Desc7.Trim()).Trim();
            shape = orderDetails.Desc2.Trim();
            machineDescription = orderDetails.Desc8.Trim();
            if (steelType != null)
                material = steelType.Description.Trim();
            else
                material = orderDetails.SteelId.Trim();
            if (!(orderDetailOptions is null))
            {
                GetOptionValues(order.OrderNumber * 100, lineItemType);
                short i = 0;
                foreach (OrderDetailOptions option in orderDetailOptions.Where(q => q.OptionCode.Trim().Length != 0))
                {
                    string optionNo = option.OptionCode.ToString().Trim();
                    optionNumbers.Add(optionNo);
                    string optionText = option.OptionText.ToString().Trim();
                    string[] printables = null;
                    string letter = optionsList.Where(x => x.OptionCode.Trim() == optionNo).FirstOrDefault().ValueType.ToString().Trim();

                    switch (letter)
                    {
                        case "A":
                            printables = !optionValuesA.Any(o => o.OptionCode.ToString().Trim() == optionNo) ? new string[] { optionText } : new string[] { optionText, " ", String.Format("{0:0.0000}", optionValuesA.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().Number1), " ", optionValuesA.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().Text.ToString().Trim() };
                            break;
                        case "B":
                            printables = !optionValuesB.Any(o => o.OptionCode.ToString().Trim() == optionNo) ? new string[] { optionText } : (optionValuesB.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().Text1.ToString().Trim() == "Diameter") && optionNo!="117" ? new string[] { optionText, " ", String.Format("{0:0.0000}", optionValuesB.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().Number1), " ", optionValuesB.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().Text1.ToString().Trim() } : new string[] { optionText, " ", String.Format("{0:0.0000}", optionValuesB.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().Number1), " ", optionValuesB.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().Text1.ToString().Trim(), " X ", String.Format("{0:0.0000}", optionValuesB.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().Number2), " ", optionValuesB.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().Text2.ToString().Trim() };
                            break;
                        case "C":
                            printables = !optionValuesC.Any(o => o.OptionCode.ToString().Trim() == optionNo) ? new string[] { optionText } : new string[] { optionText, " ", "+", String.Format("{0:0.0000}", optionValuesC.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().TopValue), "/-", String.Format("{0:0.0000}", optionValuesC.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().BottomValue) };
                            break;
                        case "D":
                            printables = !optionValuesD.Any(o => o.OptionCode.ToString().Trim() == optionNo) ? new string[] { optionText } : optionValuesD.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().Degrees == 0 && optionValuesD.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().Value == 0 ? new string[] { optionText } : new string[] { optionText, " ", String.Format("{0:0.####}", optionValuesD.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().Degrees), " Degrees X ", String.Format("{0:0.0000}", optionValuesD.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().Value), " ", optionValuesD.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().Text.ToString().Trim() };
                            break;
                        case "E":
                            printables = !optionValuesE.Any(o => o.OptionCode.ToString().Trim() == optionNo) ? new string[] { optionText } : new string[] { optionText, " ", optionValuesE.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().SmallText.ToString().Trim() };
                            break;
                        case "F":
                            printables = !optionValuesF.Any(o => o.OptionCode.ToString().Trim() == optionNo) ? new string[] { optionText } : new string[] { optionText, " ", optionValuesF.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().LargeText.ToString().Trim() };
                            break;
                        case "G":
                            printables = !optionValuesG.Any(o => o.OptionCode.ToString().Trim() == optionNo) ? new string[] { optionText } : new string[] { optionText, " ", String.Format("{0:0.####}", optionValuesG.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().Degrees), " Degrees ", optionValuesG.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().Text.ToString().Trim() };
                            break;
                        case "H":
                            printables = !optionValuesH.Any(o => o.OptionCode.ToString().Trim() == optionNo) ? new string[] { optionText } : new string[] { optionText, " ", optionValuesH.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().Hardness.ToString().Trim() };
                            break;
                        case "I":
                            printables = !optionValuesI.Any(o => o.OptionCode.ToString().Trim() == optionNo) ? new string[] { optionText } : new string[] { optionText, " ", optionValuesI.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().Hardness1.ToString().Trim(), "-", optionValuesI.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().Hardness2.ToString().Trim() };
                            break;
                        case "J":
                            printables = !optionValuesJ.Any(o => o.OptionCode.ToString().Trim() == optionNo) ? new string[] { optionText } : new string[] { optionText };
                            break;
                        case "K":
                            printables = !optionValuesK.Any(o => o.OptionCode.ToString().Trim() == optionNo) ? new string[] { optionText } : new string[] { optionText };
                            break;
                        case "L":
                            printables = !optionValuesL.Any(o => o.OptionCode.ToString().Trim() == optionNo) ? new string[] { optionText } : new string[] { optionText, " ", optionValuesL.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().SurfaceTreatment.ToString().Trim(), " ", optionValuesL.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().VendorId.ToString().Trim() };
                            break;
                        case "M":
                            printables = !optionValuesM.Any(o => o.OptionCode.ToString().Trim() == optionNo) ? new string[] { optionText } : new string[] { optionText, " ", optionValuesM.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().Screw.ToString().Trim() };
                            break;
                        case "N":
                            printables = !optionValuesN.Any(o => o.OptionCode.ToString().Trim() == optionNo) ? new string[] { optionText } : new string[] { optionText, " ", optionValuesN.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().Color.ToString().Trim() };
                            break;
                        case "O":
                            printables = !optionValuesO.Any(o => o.OptionCode.ToString().Trim() == optionNo) ? new string[] { optionText } : new string[] { optionText, " ", optionValuesO.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().Integer.ToString().Trim(), " ", optionValuesO.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().Text.ToString().Trim(), " ", String.Format("{0:0.0000}", optionValuesO.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().BoreCircle), " Bore Circle" };
                            break;
                        case "P":
                            printables = !optionValuesP.Any(o => o.OptionCode.ToString().Trim() == optionNo) ? new string[] { optionText } : new string[] { optionText, " ", String.Format("{0:0.0000}", optionValuesP.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().DegreesDecimal), " ", optionValuesP.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().Text.ToString().Trim() };
                            break;
                        case "Q":
                            printables = !optionValuesQ.Any(o => o.OptionCode.ToString().Trim() == optionNo) ? new string[] { optionText } : new string[] { optionText };
                            break;
                        case "R":
                            printables = !optionValuesR.Any(o => o.OptionCode.ToString().Trim() == optionNo) ? new string[] { optionText } : new string[] { optionText, " ", optionValuesR.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().Integer.ToString().Trim(), " ", optionValuesR.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().Text.ToString().Trim() };
                            break;
                        case "S":
                            printables = !optionValuesS.Any(o => o.OptionCode.ToString().Trim() == optionNo) ? new string[] { optionText } : new string[] { optionText };
                            break;
                        case "T":
                            printables = !optionValuesT.Any(o => o.OptionCode.ToString().Trim() == optionNo) ? new string[] { optionText } : new string[] { optionText };
                            break;
                        default:
                            printables = new string[] { optionText };
                            break;
                    }
                    if (!(printables is null) && printables.Length == 1 && printables[0] == "TIP LENGTH")
                    {
                        printables[0] = "TIP LENGTH FOR OIL SEALS";
                    }
                    Options.Add(i, printables is null ? new string[] { "" } : printables);
                    i++;
                }
            }
        }
        public void GetOptionValues(int orderNo, string lineItemType)
        {
            NAT01Context nat01Context = new NAT01Context();
            optionValuesA = nat01Context.OrdOptionValueASingleNum.Where(o => o.OrderNo == orderNo && o.OrderDetailType.Trim() == lineItemType).ToList();
            optionValuesB = nat01Context.OrdOptionValueBDoubleNum.Where(o => o.OrderNo == orderNo && o.OrderDetailType.Trim() == lineItemType).ToList();
            optionValuesC = nat01Context.OrdOptionValueCTolerance.Where(o => o.OrderNo == orderNo && o.OrderDetailType.Trim() == lineItemType).ToList();
            optionValuesD = nat01Context.OrdOptionValueDDegreeVal.Where(o => o.OrderNo == orderNo && o.OrderDetailType.Trim() == lineItemType).ToList();
            optionValuesE = nat01Context.OrdOptionValueESmallText.Where(o => o.OrderNo == orderNo && o.OrderDetailType.Trim() == lineItemType).ToList();
            optionValuesF = nat01Context.OrdOptionValueFLargeText.Where(o => o.OrderNo == orderNo && o.OrderDetailType.Trim() == lineItemType).ToList();
            optionValuesG = nat01Context.OrdOptionValueGDegrees.Where(o => o.OrderNo == orderNo && o.OrderDetailType.Trim() == lineItemType).ToList();
            optionValuesH = nat01Context.OrdOptionValueHHardness.Where(o => o.OrderNo == orderNo && o.OrderDetailType.Trim() == lineItemType).ToList();
            optionValuesI = nat01Context.OrdOptionValueIHardness2.Where(o => o.OrderNo == orderNo && o.OrderDetailType.Trim() == lineItemType).ToList();
            optionValuesJ = nat01Context.OrdOptionValueJOptionMult.Where(o => o.OrderNo == orderNo && o.OrderDetailType.Trim() == lineItemType).ToList();
            optionValuesK = nat01Context.OrdOptionValueKVendor.Where(o => o.OrderNo == orderNo && o.OrderDetailType.Trim() == lineItemType).ToList();
            optionValuesL = nat01Context.OrdOptionValueLSurfaceTreat.Where(o => o.OrderNo == orderNo && o.OrderDetailType.Trim() == lineItemType).ToList();
            optionValuesM = nat01Context.OrdOptionValueMScrew.Where(o => o.OrderNo == orderNo && o.OrderDetailType.Trim() == lineItemType).ToList();
            optionValuesN = nat01Context.OrdOptionValueNColor.Where(o => o.OrderNo == orderNo && o.OrderDetailType.Trim() == lineItemType).ToList();
            optionValuesO = nat01Context.OrdOptionValueOInteger.Where(o => o.OrderNo == orderNo && o.OrderDetailType.Trim() == lineItemType).ToList();
            optionValuesP = nat01Context.OrdOptionValuePDegDec.Where(o => o.OrderNo == orderNo && o.OrderDetailType.Trim() == lineItemType).ToList();
            optionValuesQ = nat01Context.OrdOptionValueQDimensions.Where(o => o.OrderNo == orderNo && o.OrderDetailType.Trim() == lineItemType).ToList();
            optionValuesR = nat01Context.OrdOptionValueRIntegerText.Where(o => o.OrderNo == orderNo && o.OrderDetailType.Trim() == lineItemType).ToList();
            optionValuesS = nat01Context.OrdOptionValueSText.Where(o => o.OrderNo == orderNo && o.OrderDetailType.Trim() == lineItemType).ToList();
            optionValuesT = nat01Context.OrdOptionValueTDecText.Where(o => o.OrderNo == orderNo && o.OrderDetailType.Trim() == lineItemType).ToList();
            nat01Context.Dispose();
        }
    }
}
