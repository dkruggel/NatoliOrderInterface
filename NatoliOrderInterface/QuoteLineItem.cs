using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NatoliOrderInterface.Models.NAT01;
using System.Windows;

namespace NatoliOrderInterface
{
    public class QuoteLineItem
    {
        #region Declarations
        private short lineItemNumber = 0;
        private short qtyOrdered = 0;
        private readonly Dictionary<string, string> titles = new Dictionary<string, string> {
            { "A","ALIGNMENT TOOL" },
            { "CT","COPPER TABLETS" },
            { "D","DIE" },
            { "DA","DIE ASSEMBLY" },
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
            { "","" },
        };
        #endregion

        #region Gets&Sets
        public Quote Quote { get; set; }
        public short LineItemNumber { get => lineItemNumber; set => lineItemNumber = value; }
        public short QtyOrdered { get => qtyOrdered; set => qtyOrdered = value; }
        public short? QtyShipped { get; set; } = 0;
        public string LineItemType { get; set; } = "";
        public string HobNoShapeID { get; set; } = "";
        public short? MachineNo { get; set; } = 0;
        public string MachineDescription { get; set; } = "";
        public string Material { get; set; } = "";
        public double? UnitPrice { get; set; } = 0;
        public double? ExtendedPrice { get; set; }
        public char PriceOptionsAdded { get; set; } = ' ';
        public char CompletedYN { get; set; } = ' ';
        public string Desc1 { get; set; } = "";
        public string Desc2 { get; set; } = "";
        public string Desc3 { get; set; } = "";
        public string Desc4 { get; set; } = "";
        public string Desc5 { get; set; } = "";
        public string Desc6 { get; set; } = "";
        public string Desc7 { get; set; } = "";
        public char HoldFlag { get; set; } = ' ';
        public short? OptionLastLine { get; set; }
        public float? BasePrice { get; set; }
        public float? OptionsIncrements { get; set; }
        public float? OptionsPercentage { get; set; }
        public string MachinePriceCode { get; set; } = "";
        public string SteelPriceCode { get; set; } = "";
        public string ShapePriceCode { get; set; } = "";
        public bool UnitPriceOverride { get; set; } = false;
        public short? RemakeQty { get; set; }
        public bool TaxFlag { get; set; } = false;
        public float? LineTaxes { get; set; }
        public char SheetColor { get; set; } = ' ';
        public char PrintStatus { get; set; } = ' ';
        public short? Sequence { get; set; }
        public short? TipQTY { get; set; }
        public short? DieShapeID { get; set; }
        public float? DieMinorDiameter { get; set; }
        public float? DieMajorDiameter { get; set; }
        public bool FinishedGood { get; set; } = false;
        public float? BoreCircle { get; set; }
        public List<string> OptionNumbers { get; set; } = new List<string>();
        public Dictionary<string, float?> OptionPrice { get; set; } = new Dictionary<string, float?>();
        public Dictionary<string, string> OptionType { get; set; } = new Dictionary<string, string>();
        public string Title { get; set; } = "";
        public Dictionary<short, string[]> Options { get; set; } = new Dictionary<short, string[]>();
        #endregion

        #region DataTables
        QuoteDetails quoteDetails;
        List<QuoteDetailOptions> quoteDetailOptions;
        public List<QuoteOptionValueASingleNum> optionValuesA { get; set; } = null;
        public List<QuoteOptionValueBDoubleNum> optionValuesB { get; set; } = null;
        public List<QuoteOptionValueCTolerance> optionValuesC { get; set; } = null;
        public List<QuoteOptionValueDDegreeVal> optionValuesD { get; set; } = null;
        public List<QuoteOptionValueESmallText> optionValuesE { get; set; } = null;
        public List<QuoteOptionValueFLargeText> optionValuesF { get; set; } = null;
        public List<QuoteOptionValueGDegrees> optionValuesG { get; set; } = null;
        public List<QuoteOptionValueHHardness> optionValuesH { get; set; } = null;
        public List<QuoteOptionValueIHardness2> optionValuesI { get; set; } = null;
        public List<QuoteOptionValueJOptionMult> optionValuesJ { get; set; } = null;
        public List<QuoteOptionValueKVendor> optionValuesK { get; set; } = null;
        public List<QuoteOptionValueLSurfaceTreat> optionValuesL { get; set; } = null;
        public List<QuoteOptionValueMScrew> optionValuesM { get; set; } = null;
        public List<QuoteOptionValueNColor> optionValuesN { get; set; } = null;
        public List<QuoteOptionValueOInteger> optionValuesO { get; set; } = null;
        public List<QuoteOptionValuePDegDec> optionValuesP { get; set; } = null;
        public List<QuoteOptionValueQDimensions> optionValuesQ { get; set; } = null;
        public List<QuoteOptionValueRIntegerTxt> optionValuesR { get; set; } = null;
        public List<QuoteOptionValueSSmallText> optionValuesS { get; set; } = null;
        public List<QuoteOptionValueTDecText> optionValuesT { get; set; } = null;
        private NAT01Context nat01context;
        #endregion

        private List<OptionsList> optionsList = new List<OptionsList>();

        public QuoteLineItem(Quote quote, short lineItemNumber = 1)
        {
            try
            {
                this.Quote = quote;
                this.lineItemNumber = lineItemNumber;
                nat01context = quote.Nat01Context;
                quoteDetails = nat01context.QuoteDetails.Where(q => q.QuoteNo == quote.QuoteNumber && q.Revision == quote.QuoteRevNo).FirstOrDefault();
                quoteDetailOptions = nat01context.QuoteDetailOptions.Where(q => q.QuoteNumber == quote.QuoteNumber && q.RevisionNo == quote.QuoteRevNo && q.QuoteDetailLineNo == lineItemNumber).ToList();
                optionsList = nat01context.OptionsList.ToList();
                QuoteDetails quoteDetailsRow = nat01context.QuoteDetails.Where(q => q.QuoteNo == quote.QuoteNumber && q.Revision == quote.QuoteRevNo && q.LineNumber == lineItemNumber).FirstOrDefault();
                if (quoteDetailsRow != null)
                    SetInfo(quoteDetailsRow, quoteDetailOptions, optionsList);
                else
                    this.lineItemNumber = -1;
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("QuoteLineItem.cs -> QuoteNo: " + Quote.QuoteNumber + "-" + quote.QuoteRevNo + " LineNumber: "+ lineItemNumber, ex.Message, null);
            }
        }

        public void SetInfo(QuoteDetails row, List<QuoteDetailOptions> quoteDetailOptions, List<OptionsList> optionsList)
        {
            LineItemType = row.DetailTypeId.Trim();
            GetOptionValues(Quote.QuoteNumber, Quote.QuoteRevNo == null ? (short)0 : (short)Quote.QuoteRevNo, LineItemType);
            if(!short.TryParse(row.LineNumber.ToString(), out lineItemNumber))
            {
                lineItemNumber = -1;
            }
            if(!short.TryParse(row.QuantityOrdered.ToString(), out qtyOrdered))
            {
                qtyOrdered = 0;
            }
            QtyShipped = row.QuantityShipped;
            Title = titles[LineItemType];
            HobNoShapeID = row.HobNoShapeId;
            MachineNo = row.MachineNo;
            MachineDescription = row.Desc8;
            if (!(row is null))
            {
                if (nat01context.SteelType.Any(s => s.TypeId == row.SteelId.Trim()))
                {
                    SteelType mat = nat01context.SteelType.Where(s => s.TypeId == row.SteelId.Trim()).FirstOrDefault();
                    Material = mat.Description.ToString().Trim();
                }                    
                else
                {
                    Material = "";
                }
            }
            else
            {
                Material = "";
            }

            UnitPrice = row.UnitPrice;
            ExtendedPrice = row.ExtendedPrice;
            PriceOptionsAdded = row.PriceOptionsAdded is null ? ' ' : Char.Parse(row.PriceOptionsAdded);
            CompletedYN = row.CompletedYn is null ? ' ' : Char.Parse(row.CompletedYn);
            Desc1 = row.Desc1.ToString().Trim();
            Desc2 = row.Desc2.ToString().Trim();
            Desc3 = row.Desc3.ToString().Trim();
            Desc4 = row.Desc4.ToString().Trim();
            Desc5 = row.Desc5.ToString().Trim();
            Desc6 = row.Desc6.ToString().Trim();
            Desc7 = row.Desc7.ToString().Trim();
            HoldFlag = row.HoldFlag is null ? ' ' : Char.Parse(row.HoldFlag);
            OptionLastLine = row.OptionLastLine;
            BasePrice = row.BasePrice;
            OptionsIncrements = row.OptionsIncrements;
            OptionsPercentage = row.OptionsPercentage;
            MachinePriceCode = row.MachinePriceCode.ToString().Trim();
            SteelPriceCode = row.SteelPriceCode.ToString().Trim();
            ShapePriceCode = row.ShapePriceCode.ToString().Trim();
            UnitPriceOverride = bool.Parse(row.UnitPriceOverride.ToString());
            RemakeQty = row.RemakeQty;
            TaxFlag = bool.Parse(row.TaxFlag.ToString());
            LineTaxes = row.LineTaxes;
            SheetColor = Char.Parse(row.SheetColor);
            PrintStatus = row.PrintStatus is null ? ' ' : Char.Parse(row.PrintStatus);
            Sequence = row.Sequence;
            TipQTY = row.TipQty;
            DieShapeID = row.DieShapeId;
            DieMinorDiameter = row.DieMinorDiameter;
            DieMajorDiameter = row.DieMajorDiameter;
            FinishedGood = bool.Parse(row.FinishedGood.ToString());
            BoreCircle = row.BoreCircle;
            try
            {
                if (!(quoteDetailOptions is null))
                {
                    if (Quote.UnitOfMeasure != 'M')
                    {
                        short i = 0;
                        foreach (QuoteDetailOptions option in quoteDetailOptions.Where(q => q.OptionCode.Trim().Length != 0))
                        {
                            string optionNo = option.OptionCode.ToString().Trim();
                            string optionText = option.OptionText.ToString().Trim();
                            string[] printables = null;
                            string letter = optionsList.Where(x => x.OptionCode.Trim() == optionNo).FirstOrDefault().ValueType.ToString().Trim();
                            switch (letter)
                            {
                                case "A":
                                    printables = !optionValuesA.Any(o => o.OptionCode.ToString().Trim() == optionNo) ? new string[] { optionText } : new string[] { optionText, " ", String.Format("{0:0.0000}", optionValuesA.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().Number1), " ", optionValuesA.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().Text.ToString().Trim() };
                                    break;
                                case "B":
                                    printables = !optionValuesB.Any(o => o.OptionCode.ToString().Trim() == optionNo) ? new string[] { optionText } : optionValuesB.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().Text1.ToString().Trim() == "Diameter" && optionNo != "117" && optionNo != "463" ? new string[] { optionText, " ", String.Format("{0:0.0000}", optionValuesB.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().Number1), " ", optionValuesB.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().Text1.ToString().Trim() } : new string[] { optionText, " ", String.Format("{0:0.0000}", optionValuesB.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().Number1), " ", optionValuesB.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().Text1.ToString().Trim(), " X ", String.Format("{0:0.0000}", optionValuesB.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().Number2), " ", optionValuesB.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().Text2.ToString().Trim() };
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
                            OptionPrice.Add(optionNo, option.OptionType == "P" ? option.OrdDetOptPercnt : option.OrdDetOptPrice);
                            OptionType.Add(optionNo, option.OptionType);
                            OptionNumbers.Add(optionNo);
                            Options.Add(i, printables is null ? new string[] { "" } : printables);
                            i++;
                        }
                    }
                    else
                    {
                        short i = 0;
                        foreach (QuoteDetailOptions option in quoteDetailOptions.Where(q => q.OptionCode.Trim().Length != 0))
                        {
                            string optionNo = option.OptionCode.ToString().Trim();
                            string optionText = option.OptionText.ToString().Trim();
                            string[] printables = null;
                            string letter = optionsList.Where(x => x.OptionCode.Trim() == optionNo).FirstOrDefault().ValueType.ToString().Trim();
                            switch (letter)
                            {
                                case "A":
                                    printables = !optionValuesA.Any(o => o.OptionCode.ToString().Trim() == optionNo) ? new string[] { optionText } : new string[] { optionText, " ", String.Format("{0:0.000}", optionValuesA.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().Number1Mm) + "mm ", "(" + String.Format("{0:0.0000}", optionValuesA.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().Number1) + "\")", " ", optionValuesA.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().Text.ToString().Trim() };
                                    break;
                                case "B":
                                    printables = !optionValuesB.Any(o => o.OptionCode.ToString().Trim() == optionNo) ? new string[] { optionText } : optionValuesB.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().Text1.ToString().Trim() == "Diameter" ? new string[] { optionText, " ", String.Format("{0:0.000}", optionValuesB.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().Number1Mm) + "mm ", "(" + String.Format("{0:0.0000}", optionValuesB.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().Number1) + "\")", " ", optionValuesB.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().Text1.ToString().Trim() } : new string[] { optionText, " ", String.Format("{0:0.000}", optionValuesB.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().Number1Mm) + "mm ", "(" + String.Format("{0:0.0000}", optionValuesB.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().Number1) + "\")", " ", optionValuesB.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().Text1.ToString().Trim(), " X ", String.Format("{0:0.000}", optionValuesB.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().Number2Mm) + "mm ", "(" + String.Format("{0:0.0000}", optionValuesB.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().Number2) + "\")", " ", optionValuesB.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().Text2.ToString().Trim() };
                                    break;
                                case "C":
                                    printables = !optionValuesC.Any(o => o.OptionCode.ToString().Trim() == optionNo) ? new string[] { optionText } : new string[] { optionText, " ", "+", String.Format("{0:0.000}", optionValuesC.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().TopMm), "/-", String.Format("{0:0.000}", optionValuesC.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().BottomMm) + "mm ", "(+", String.Format("{0:0.0000}", optionValuesC.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().TopValue), "/-", String.Format("{0:0.0000}", optionValuesC.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().BottomValue) + "\")" };
                                    break;
                                case "D":
                                    printables = !optionValuesD.Any(o => o.OptionCode.ToString().Trim() == optionNo) ? new string[] { optionText } : optionValuesD.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().Degrees == 0 && optionValuesD.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().Value == 0 ? new string[] { optionText } : new string[] { optionText, " ", String.Format("{0:0.####}", optionValuesD.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().Degrees), " Degrees X ", String.Format("{0:0.000}", optionValuesD.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().ValueMm) + "mm ", "(" + String.Format("{0:0.0000}", optionValuesD.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().Value) + "\")", " ", optionValuesD.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().Text.ToString().Trim() };
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
                                    printables = !optionValuesO.Any(o => o.OptionCode.ToString().Trim() == optionNo) ? new string[] { optionText } : new string[] { optionText, " ", optionValuesO.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().Integer.ToString().Trim(), " ", optionValuesO.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().Text.ToString().Trim(), " ", "(" + String.Format("{0:0.0000}", optionValuesO.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().BoreCircle) + "\")", " Bore Circle" };
                                    break;
                                case "P":
                                    printables = !optionValuesP.Any(o => o.OptionCode.ToString().Trim() == optionNo) ? new string[] { optionText } : new string[] { optionText, " ", "(" + String.Format("{0:0.0000}", optionValuesP.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().DegreesDecimal) + (optionValuesP.Where(o => o.OptionCode.ToString().Trim() == optionNo && !string.IsNullOrEmpty(o.Text) && !string.IsNullOrWhiteSpace(o.Text)).FirstOrDefault().Text.Trim() == "INCH" ? "\"" :"")+")", " ", optionValuesP.Where(o => o.OptionCode.ToString().Trim() == optionNo).FirstOrDefault().Text.ToString().Trim() };
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
                            OptionPrice.Add(optionNo, option.OptionType == "P" ? option.OrdDetOptPercnt : option.OrdDetOptPrice);
                            OptionType.Add(optionNo, option.OptionType);
                            OptionNumbers.Add(optionNo);
                            Options.Add(i, printables is null ? new string[] { "" } : printables);
                            i++;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public void GetOptionValues(int quoteNo, short revNo, string lineType)
        {
            optionValuesA = nat01context.QuoteOptionValueASingleNum.Where(q => q.QuoteNo == quoteNo && q.RevNo == revNo && q.QuoteDetailType.Trim() == lineType).ToList();
            optionValuesB = nat01context.QuoteOptionValueBDoubleNum.Where(q => q.QuoteNo == quoteNo && q.RevNo == revNo && q.QuoteDetailType.Trim() == lineType).ToList();
            optionValuesC = nat01context.QuoteOptionValueCTolerance.Where(q => q.QuoteNo == quoteNo && q.RevNo == revNo && q.QuoteDetailType.Trim() == lineType).ToList();
            optionValuesD = nat01context.QuoteOptionValueDDegreeVal.Where(q => q.QuoteNo == quoteNo && q.RevNo == revNo && q.QuoteDetailType.Trim() == lineType).ToList();
            optionValuesE = nat01context.QuoteOptionValueESmallText.Where(q => q.QuoteNo == quoteNo && q.RevNo == revNo && q.QuoteDetailType.Trim() == lineType).ToList();
            optionValuesF = nat01context.QuoteOptionValueFLargeText.Where(q => q.QuoteNo == quoteNo && q.RevNo == revNo && q.QuoteDetailType.Trim() == lineType).ToList();
            optionValuesG = nat01context.QuoteOptionValueGDegrees.Where(q => q.QuoteNo == quoteNo && q.RevNo == revNo && q.QuoteDetailType.Trim() == lineType).ToList();
            optionValuesH = nat01context.QuoteOptionValueHHardness.Where(q => q.QuoteNo == quoteNo && q.RevNo == revNo && q.QuoteDetailType.Trim() == lineType).ToList();
            optionValuesI = nat01context.QuoteOptionValueIHardness2.Where(q => q.QuoteNo == quoteNo && q.RevNo == revNo && q.QuoteDetailType.Trim() == lineType).ToList();
            optionValuesJ = nat01context.QuoteOptionValueJOptionMult.Where(q => q.QuoteNo == quoteNo && q.RevNo == revNo && q.QuoteDetailType.Trim() == lineType).ToList();
            optionValuesK = nat01context.QuoteOptionValueKVendor.Where(q => q.QuoteNo == quoteNo && q.RevNo == revNo && q.QuoteDetailType.Trim() == lineType).ToList();
            optionValuesL = nat01context.QuoteOptionValueLSurfaceTreat.Where(q => q.QuoteNo == quoteNo && q.RevNo == revNo && q.QuoteDetailType.Trim() == lineType).ToList();
            optionValuesM = nat01context.QuoteOptionValueMScrew.Where(q => q.QuoteNo == quoteNo && q.RevNo == revNo && q.QuoteDetailType.Trim() == lineType).ToList();
            optionValuesN = nat01context.QuoteOptionValueNColor.Where(q => q.QuoteNo == quoteNo && q.RevNo == revNo && q.QuoteDetailType.Trim() == lineType).ToList();
            optionValuesO = nat01context.QuoteOptionValueOInteger.Where(q => q.QuoteNo == quoteNo && q.RevNo == revNo && q.QuoteDetailType.Trim() == lineType).ToList();
            optionValuesP = nat01context.QuoteOptionValuePDegDec.Where(q => q.QuoteNo == quoteNo && q.RevNo == revNo && q.QuoteDetailType.Trim() == lineType).ToList();
            optionValuesQ = nat01context.QuoteOptionValueQDimensions.Where(q => q.QuoteNo == quoteNo && q.RevNo == revNo && q.QuoteDetailType.Trim() == lineType).ToList();
            optionValuesR = nat01context.QuoteOptionValueRIntegerTxt.Where(q => q.QuoteNo == quoteNo && q.RevNo == revNo && q.QuoteDetailType.Trim() == lineType).ToList();
            optionValuesS = nat01context.QuoteOptionValueSSmallText.Where(q => q.QuoteNo == quoteNo && q.RevNo == revNo && q.QuoteDetailType.Trim() == lineType).ToList();
            optionValuesT = nat01context.QuoteOptionValueTDecText.Where(q => q.QuoteNo == quoteNo && q.RevNo == revNo && q.QuoteDetailType.Trim() == lineType).ToList();

        }
        public bool IsKeyed()
        {
            return this.OptionNumbers.Contains("130") || this.OptionNumbers.Contains("131") || this.OptionNumbers.Contains("132") || this.OptionNumbers.Contains("133") || this.OptionNumbers.Contains("140") || this.OptionNumbers.Contains("141");
        }
    }
}
