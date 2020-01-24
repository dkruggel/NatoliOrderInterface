using System;
using System.Collections.Generic;
using System.Text;
using NatoliOrderInterface.Models;
using NatoliOrderInterface.Models.NAT01;
using NatoliOrderInterface.Models.NEC;
using System.Linq;

namespace NatoliOrderInterface
{
    public class Quote: IDisposable
    {
        #region Table Columns
        private int quoteNumber = 0;
        public int QuoteNumber
        {
            get { return quoteNumber; }
            set { this.quoteNumber = value; }
        }

        private string customerNo = "";
        public string CustomerNo
        {
            get { return customerNo; }
            set { this.customerNo = value; }
        }

        private string shipToAccountNo = "";
        public string ShipToAccountNo
        {
            get { return shipToAccountNo; }
            set { this.shipToAccountNo = value; }
        }

        private string shipToNo = "";
        public string ShipToNo
        {
            get { return shipToNo; }
            set { this.shipToNo = value; }
        }

        private short? lastLineNo = 0;
        public short? LastLineNo
        {
            get { return lastLineNo; }
            set { this.lastLineNo = value; }
        }

        private short? lastNoteNo = 0;
        public short? LastNoteNo
        {
            get { return lastNoteNo; }
            set { this.lastNoteNo = value; }
        }

        private DateTime quoteDate = DateTime.MinValue;
        public DateTime QuoteDate
        {
            get { return quoteDate; }
            set { this.quoteDate = value; }
        }

        private double? quoteMarkdown = 0.0;
        public double? QuoteMarkdown
        {
            get { return quoteMarkdown; }
            set { this.quoteMarkdown = value; }
        }

        private double quoteSubtotal = 0.0;
        public double QuoteSubtotal
        {
            get { return quoteSubtotal; }
            set { this.quoteSubtotal = value; }
        }

        private double? quoteFreightCharge = 0.0;
        public double? QuoteFreightCharge
        {
            get { return quoteFreightCharge; }
            set { this.quoteFreightCharge = value; }
        }

        private double? orderTax = 0.0;
        public double? OrderTax
        {
            get { return orderTax; }
            set { this.orderTax = value; }
        }

        private double quoteTotal = 0.0;
        public double QuoteTotal
        {
            get { return quoteTotal; }
            set { this.quoteTotal = value; }
        }

        private string reference = "";
        public string Reference
        {
            get { return reference; }
            set { this.reference = value; }
        }

        private string pOExtension = "";
        public string POExtension
        {
            get { return pOExtension; }
            set { this.pOExtension = value; }
        }

        private string shippedVia = "";
        public string ShippedVia
        {
            get { return shippedVia; }
            set { this.shippedVia = value; }
        }

        private string quoteRepID = "";
        public string QuoteRepID
        {
            get { return quoteRepID; }
            set { this.quoteRepID = value; }
        }

        private string termsID = "";
        public string TermsID
        {
            get { return termsID; }
            set { this.termsID = value; }
        }

        private char freightDescID = ' ';
        public char FreightDescID
        {
            get { return freightDescID; }
            set { this.freightDescID = value; }
        }

        private char printStatus = ' ';
        public char PrintStatus
        {
            get { return printStatus; }
            set { this.printStatus = value; }
        }

        private char revisedYN = ' ';
        public char RevisedYN
        {
            get { return revisedYN; }
            set { this.revisedYN = value; }
        }

        private char shippedYN = ' ';
        public char ShippedYN
        {
            get { return shippedYN; }
            set { this.shippedYN = value; }
        }

        private string note1 = "";
        public string Note1
        {
            get { return note1; }
            set { this.note1 = value; }
        }

        private string note2 = "";
        public string Note2
        {
            get { return note2; }
            set { this.note2 = value; }
        }

        private string shiptoName = "";
        public string ShiptoName
        {
            get { return shiptoName; }
            set { this.shiptoName = value; }
        }

        private string shiptoAddr1 = "";
        public string ShiptoAddr1
        {
            get { return shiptoAddr1; }
            set { this.shiptoAddr1 = value; }
        }

        private string shiptoAddr2 = "";
        public string ShiptoAddr2
        {
            get { return shiptoAddr2; }
            set { this.shiptoAddr2 = value; }
        }

        private string shiptoAddr3 = "";
        public string ShiptoAddr3
        {
            get { return shiptoAddr3; }
            set { this.shiptoAddr3 = value; }
        }

        private string shiptoCity = "";
        public string ShiptoCity
        {
            get { return shiptoCity; }
            set { this.shiptoCity = value; }
        }

        private string shiptoState = "";
        public string ShiptoState
        {
            get { return shiptoState; }
            set { this.shiptoState = value; }
        }

        private string shiptoZip = "";
        public string ShiptoZip
        {
            get { return shiptoZip; }
            set { this.shiptoZip = value; }
        }

        private string shiptoCountry = "";
        public string ShiptoCountry
        {
            get { return shiptoCountry; }
            set { this.shiptoCountry = value; }
        }

        private string shipToPhone = "";
        public string ShipToPhone
        {
            get { return shipToPhone; }
            set { this.shipToPhone = value; }
        }

        private DateTime? requestedShipDate = DateTime.MinValue;
        public DateTime? RequestedShipDate
        {
            get { return requestedShipDate; }
            set { this.requestedShipDate = value; }
        }

        private string contactPerson = "";
        public string ContactPerson
        {
            get { return contactPerson; }
            set { this.contactPerson = value; }
        }

        private string shipToFax = "";
        public string ShipToFax
        {
            get { return shipToFax; }
            set { this.shipToFax = value; }
        }

        private short? lastWONoteNo = 0;
        public short? LastWONoteNo
        {
            get { return lastWONoteNo; }
            set { this.lastWONoteNo = value; }
        }

        private char printQuoteStatus = ' ';
        public char PrintQuoteStatus
        {
            get { return printQuoteStatus; }
            set { this.printQuoteStatus = value; }
        }

        private char convertedToOrder = ' ';
        public char ConvertedToOrder
        {
            get { return convertedToOrder; }
            set { this.convertedToOrder = value; }
        }

        private char printPkgSlipStat = ' ';
        public char PrintPkgSlipStat
        {
            get { return printPkgSlipStat; }
            set { this.printPkgSlipStat = value; }
        }

        private char printMiniWorkOrd = ' ';
        public char PrintMiniWorkOrd
        {
            get { return printMiniWorkOrd; }
            set { this.printMiniWorkOrd = value; }
        }

        private char print_Misc1 = ' ';
        public char Print_Misc1
        {
            get { return print_Misc1; }
            set { this.print_Misc1 = value; }
        }

        private char print_Misc2 = ' ';
        public char Print_Misc2
        {
            get { return print_Misc2; }
            set { this.print_Misc2 = value; }
        }

        private char print_Misc3 = ' ';
        public char Print_Misc3
        {
            get { return print_Misc3; }
            set { this.print_Misc3 = value; }
        }

        private char print_Misc4 = ' ';
        public char Print_Misc4
        {
            get { return print_Misc4; }
            set { this.print_Misc4 = value; }
        }

        private char print_Misc5 = ' ';
        public char Print_Misc5
        {
            get { return print_Misc5; }
            set { this.print_Misc5 = value; }
        }

        private string productName = "";
        public string ProductName
        {
            get { return productName; }
            set { this.productName = value; }
        }

        private string hobRequired = "";
        public string HobRequired
        {
            get { return hobRequired; }
            set { this.hobRequired = value; }
        }

        private short? quoteRevNo = 0;
        public short? QuoteRevNo
        {
            get { return quoteRevNo; }
            set { this.quoteRevNo = value; }
        }

        private char workOrderType = ' ';
        public char WorkOrderType
        {
            get { return workOrderType; }
            set { this.workOrderType = value; }
        }

        private string engineeringNote1 = "";
        public string EngineeringNote1
        {
            get { return engineeringNote1; }
            set { this.engineeringNote1 = value; }
        }

        private string engineeringNote2 = "";
        public string EngineeringNote2
        {
            get { return engineeringNote2; }
            set { this.engineeringNote2 = value; }
        }

        private string miscNote = "";
        public string MiscNote
        {
            get { return miscNote; }
            set { this.miscNote = value; }
        }

        private string etchingNote = "";
        public string EtchingNote
        {
            get { return etchingNote; }
            set { this.etchingNote = value; }
        }

        private string shippingNote = "";
        public string ShippingNote
        {
            get { return shippingNote; }
            set { this.shippingNote = value; }
        }

        private string inspectionNote = "";
        public string InspectionNote
        {
            get { return inspectionNote; }
            set { this.inspectionNote = value; }
        }

        private double? orderNo = 0.0;
        public double? OrderNo
        {
            get { return orderNo; }
            set { this.orderNo = value; }
        }

        private string shipment = "";
        public string Shipment
        {
            get { return shipment; }
            set { this.shipment = value; }
        }

        private string billToName = "";
        public string BillToName
        {
            get { return billToName; }
            set { this.billToName = value; }
        }

        private string billToAddr1 = "";
        public string BillToAddr1
        {
            get { return billToAddr1; }
            set { this.billToAddr1 = value; }
        }

        private string billToAddr2 = "";
        public string BillToAddr2
        {
            get { return billToAddr2; }
            set { this.billToAddr2 = value; }
        }

        private string billToAddr3 = "";
        public string BillToAddr3
        {
            get { return billToAddr3; }
            set { this.billToAddr3 = value; }
        }

        private string billToCity = "";
        public string BillToCity
        {
            get { return billToCity; }
            set { this.billToCity = value; }
        }

        private string billToState = "";
        public string BillToState
        {
            get { return billToState; }
            set { this.billToState = value; }
        }

        private string billToZip = "";
        public string BillToZip
        {
            get { return billToZip; }
            set { this.billToZip = value; }
        }

        private string billToCountry = "";
        public string BillToCountry
        {
            get { return billToCountry; }
            set { this.billToCountry = value; }
        }

        private string pONumber = "";
        public string PONumber
        {
            get { return pONumber; }
            set { this.pONumber = value; }
        }

        private string internationalID = "";
        public string InternationalID
        {
            get { return internationalID; }
            set { this.internationalID = value; }
        }

        private string email = "";
        public string Email
        {
            get { return email; }
            set { this.email = value; }
        }

        private string emailName = "";
        public string EmailName
        {
            get { return emailName; }
            set { this.emailName = value; }
        }

        private string email2 = "";
        public string Email2
        {
            get { return email2; }
            set { this.email2 = value; }
        }

        private string email2Name = "";
        public string Email2Name
        {
            get { return email2Name; }
            set { this.email2Name = value; }
        }

        private string email3 = "";
        public string Email3
        {
            get { return email3; }
            set { this.email3 = value; }
        }

        private string email3Name = "";
        public string Email3Name
        {
            get { return email3Name; }
            set { this.email3Name = value; }
        }

        private string email4 = "";
        public string Email4
        {
            get { return email4; }
            set { this.email4 = value; }
        }

        private string email4Name = "";
        public string Email4Name
        {
            get { return email4Name; }
            set { this.email4Name = value; }
        }

        private string email5 = "";
        public string Email5
        {
            get { return email5; }
            set { this.email5 = value; }
        }

        private string email5Name = "";
        public string Email5Name
        {
            get { return email5Name; }
            set { this.email5Name = value; }
        }

        private char rushYorN = ' ';
        public char RushYorN
        {
            get { return rushYorN; }
            set { this.rushYorN = value; }
        }

        private string rushNatoliContact = "";
        public string RushNatoliContact
        {
            get { return rushNatoliContact; }
            set { this.rushNatoliContact = value; }
        }

        private char unitOfMeasure = ' ';
        public char UnitOfMeasure
        {
            get { return unitOfMeasure; }
            set { this.unitOfMeasure = value; }
        }

        private char iTNRequired = ' ';
        public char ITNRequired
        {
            get { return iTNRequired; }
            set { this.iTNRequired = value; }
        }

        private string commInvType = "";
        public string CommInvType
        {
            get { return commInvType; }
            set { this.commInvType = value; }
        }

        private string incoterms = "";
        public string Incoterms
        {
            get { return incoterms; }
            set { this.incoterms = value; }
        }

        private char shippingBillMethod = ' ';
        public char ShippingBillMethod
        {
            get { return shippingBillMethod; }
            set { this.shippingBillMethod = value; }
        }

        private string shippingAcctNo = "";
        public string ShippingAcctNo
        {
            get { return shippingAcctNo; }
            set { this.shippingAcctNo = value; }
        }

        private string dutiesTaxesBilling = "";
        public string DutiesTaxesBilling
        {
            get { return dutiesTaxesBilling; }
            set { this.dutiesTaxesBilling = value; }
        }

        private string dutiesTaxesAcctNo = "";
        public string DutiesTaxesAcctNo
        {
            get { return dutiesTaxesAcctNo; }
            set { this.dutiesTaxesAcctNo = value; }
        }

        private string exportReason = "";
        public string ExportReason
        {
            get { return exportReason; }
            set { this.exportReason = value; }
        }

        private string logisticsContact = "";
        public string LogisticsContact
        {
            get { return logisticsContact; }
            set { this.logisticsContact = value; }
        }

        private string logisticsEmail = "";
        public string LogisticsEmail
        {
            get { return logisticsEmail; }
            set { this.logisticsEmail = value; }
        }

        private char internationalYorN = ' ';
        public char InternationalYorN
        {
            get { return internationalYorN; }
            set { this.internationalYorN = value; }
        }

        private char pricing = ' ';
        public char Pricing
        {
            get { return pricing; }
            set { this.pricing = value; }
        }

        private char newCustomer = ' ';
        public char NewCustomer
        {
            get { return newCustomer; }
            set { this.newCustomer = value; }
        }

        private char exportedToEng = ' ';
        public char ExportedToEng
        {
            get { return exportedToEng; }
            set { this.exportedToEng = value; }
        }

        private string logisticsPhoneNumber = "";
        public string LogisticsPhoneNumber
        {
            get { return logisticsPhoneNumber; }
            set { this.logisticsPhoneNumber = value; }
        }

        private string iTNNo = "";
        public string ITNNo
        {
            get { return iTNNo; }
            set { this.iTNNo = value; }
        }

        private string currencyID = "";
        public string CurrencyID
        {
            get { return currencyID; }
            set { this.currencyID = value; }
        }

        private double? exchangeRate = 0.0;
        public double? ExchangeRate
        {
            get { return exchangeRate; }
            set { this.exchangeRate = value; }
        }

        private string uEtching1 = "";
        public string UEtching1
        {
            get { return uEtching1; }
            set { this.uEtching1 = value; }
        }

        private string uEtching2 = "";
        public string UEtching2
        {
            get { return uEtching2; }
            set { this.uEtching2 = value; }
        }

        private string uEtching3 = "";
        public string UEtching3
        {
            get { return uEtching3; }
            set { this.uEtching3 = value; }
        }

        private string uEtching4 = "";
        public string UEtching4
        {
            get { return uEtching4; }
            set { this.uEtching4 = value; }
        }

        private string uEtching5 = "";
        public string UEtching5
        {
            get { return uEtching5; }
            set { this.uEtching5 = value; }
        }

        private string uEtching6 = "";
        public string UEtching6
        {
            get { return uEtching6; }
            set { this.uEtching6 = value; }
        }

        private string lEtching1 = "";
        public string LEtching1
        {
            get { return lEtching1; }
            set { this.lEtching1 = value; }
        }

        private string lEtching2 = "";
        public string LEtching2
        {
            get { return lEtching2; }
            set { this.lEtching2 = value; }
        }

        private string lEtching3 = "";
        public string LEtching3
        {
            get { return lEtching3; }
            set { this.lEtching3 = value; }
        }

        private string lEtching4 = "";
        public string LEtching4
        {
            get { return lEtching4; }
            set { this.lEtching4 = value; }
        }

        private string lEtching5 = "";
        public string LEtching5
        {
            get { return lEtching5; }
            set { this.lEtching5 = value; }
        }

        private string lEtching6 = "";
        public string LEtching6
        {
            get { return lEtching6; }
            set { this.lEtching6 = value; }
        }

        private string dEtching1 = "";
        public string DEtching1
        {
            get { return dEtching1; }
            set { this.dEtching1 = value; }
        }

        private string dEtching2 = "";
        public string DEtching2
        {
            get { return dEtching2; }
            set { this.dEtching2 = value; }
        }

        private string dEtching3 = "";
        public string DEtching3
        {
            get { return dEtching3; }
            set { this.dEtching3 = value; }
        }

        private string dEtching4 = "";
        public string DEtching4
        {
            get { return dEtching4; }
            set { this.dEtching4 = value; }
        }

        private string dEtching5 = "";
        public string DEtching5
        {
            get { return dEtching5; }
            set { this.dEtching5 = value; }
        }

        private string dEtching6 = "";
        public string DEtching6
        {
            get { return dEtching6; }
            set { this.dEtching6 = value; }
        }

        private string rEtching1 = "";
        public string REtching1
        {
            get { return rEtching1; }
            set { this.rEtching1 = value; }
        }

        private string rEtching2 = "";
        public string REtching2
        {
            get { return rEtching2; }
            set { this.rEtching2 = value; }
        }

        private string rEtching3 = "";
        public string REtching3
        {
            get { return rEtching3; }
            set { this.rEtching3 = value; }
        }

        private string rEtching4 = "";
        public string REtching4
        {
            get { return rEtching4; }
            set { this.rEtching4 = value; }
        }

        private string rEtching5 = "";
        public string REtching5
        {
            get { return rEtching5; }
            set { this.rEtching5 = value; }
        }

        private string rEtching6 = "";
        public string REtching6
        {
            get { return rEtching6; }
            set { this.rEtching6 = value; }
        }

        private string userAcctNo = "";
        public string UserAcctNo
        {
            get { return userAcctNo; }
            set { this.userAcctNo = value; }
        }

        private string userLocNo = "";
        public string UserLocNo
        {
            get { return userLocNo; }
            set { this.userLocNo = value; }
        }

        private string uEtching1B = "";
        public string UEtching1B
        {
            get { return uEtching1B; }
            set { this.uEtching1B = value; }
        }

        private string uEtching2B = "";
        public string UEtching2B
        {
            get { return uEtching2B; }
            set { this.uEtching2B = value; }
        }

        private string uEtching3B = "";
        public string UEtching3B
        {
            get { return uEtching3B; }
            set { this.uEtching3B = value; }
        }

        private string uEtching4B = "";
        public string UEtching4B
        {
            get { return uEtching4B; }
            set { this.uEtching4B = value; }
        }

        private string uEtching5B = "";
        public string UEtching5B
        {
            get { return uEtching5B; }
            set { this.uEtching5B = value; }
        }

        private string uEtching6B = "";
        public string UEtching6B
        {
            get { return uEtching6B; }
            set { this.uEtching6B = value; }
        }

        private string lEtching1B = "";
        public string LEtching1B
        {
            get { return lEtching1B; }
            set { this.lEtching1B = value; }
        }

        private string lEtching2B = "";
        public string LEtching2B
        {
            get { return lEtching2B; }
            set { this.lEtching2B = value; }
        }

        private string lEtching3B = "";
        public string LEtching3B
        {
            get { return lEtching3B; }
            set { this.lEtching3B = value; }
        }

        private string lEtching4B = "";
        public string LEtching4B
        {
            get { return lEtching4B; }
            set { this.lEtching4B = value; }
        }

        private string lEtching5B = "";
        public string LEtching5B
        {
            get { return lEtching5B; }
            set { this.lEtching5B = value; }
        }

        private string lEtching6B = "";
        public string LEtching6B
        {
            get { return lEtching6B; }
            set { this.lEtching6B = value; }
        }

        private string dEtching1B = "";
        public string DEtching1B
        {
            get { return dEtching1B; }
            set { this.dEtching1B = value; }
        }

        private string dEtching2B = "";
        public string DEtching2B
        {
            get { return dEtching2B; }
            set { this.dEtching2B = value; }
        }

        private string dEtching3B = "";
        public string DEtching3B
        {
            get { return dEtching3B; }
            set { this.dEtching3B = value; }
        }

        private string dEtching4B = "";
        public string DEtching4B
        {
            get { return dEtching4B; }
            set { this.dEtching4B = value; }
        }

        private string dEtching5B = "";
        public string DEtching5B
        {
            get { return dEtching5B; }
            set { this.dEtching5B = value; }
        }

        private string dEtching6B = "";
        public string DEtching6B
        {
            get { return dEtching6B; }
            set { this.dEtching6B = value; }
        }

        private string rEtching1B = "";
        public string REtching1B
        {
            get { return rEtching1B; }
            set { this.rEtching1B = value; }
        }

        private string rEtching2B = "";
        public string REtching2B
        {
            get { return rEtching2B; }
            set { this.rEtching2B = value; }
        }

        private string rEtching3B = "";
        public string REtching3B
        {
            get { return rEtching3B; }
            set { this.rEtching3B = value; }
        }

        private string rEtching4B = "";
        public string REtching4B
        {
            get { return rEtching4B; }
            set { this.rEtching4B = value; }
        }

        private string rEtching5B = "";
        public string REtching5B
        {
            get { return rEtching5B; }
            set { this.rEtching5B = value; }
        }

        private string rEtching6B = "";
        public string REtching6B
        {
            get { return rEtching6B; }
            set { this.rEtching6B = value; }
        }

        private string aEtching1 = "";
        public string AEtching1
        {
            get { return aEtching1; }
            set { this.aEtching1 = value; }
        }

        private string aEtching2 = "";
        public string AEtching2
        {
            get { return aEtching2; }
            set { this.aEtching2 = value; }
        }

        private string aEtching3 = "";
        public string AEtching3
        {
            get { return aEtching3; }
            set { this.aEtching3 = value; }
        }

        private string aEtching4 = "";
        public string AEtching4
        {
            get { return aEtching4; }
            set { this.aEtching4 = value; }
        }

        private string aEtching5 = "";
        public string AEtching5
        {
            get { return aEtching5; }
            set { this.aEtching5 = value; }
        }

        private string aEtching6 = "";
        public string AEtching6
        {
            get { return aEtching6; }
            set { this.aEtching6 = value; }
        }

        private string origination = "";
        public string Origination
        {
            get { return origination; }
            set { this.origination = value; }
        }

        private string uEtching7 = "";
        public string UEtching7
        {
            get { return uEtching7; }
            set { this.uEtching7 = value; }
        }

        private string lEtching7 = "";
        public string LEtching7
        {
            get { return lEtching7; }
            set { this.lEtching7 = value; }
        }

        private string dEtching7 = "";
        public string DEtching7
        {
            get { return dEtching7; }
            set { this.dEtching7 = value; }
        }

        private string rEtching7 = "";
        public string REtching7
        {
            get { return rEtching7; }
            set { this.rEtching7 = value; }
        }

        private string uEtching7B = "";
        public string UEtching7B
        {
            get { return uEtching7B; }
            set { this.uEtching7B = value; }
        }

        private string lEtching7B = "";
        public string LEtching7B
        {
            get { return lEtching7B; }
            set { this.lEtching7B = value; }
        }

        private string dEtching7B = "";
        public string DEtching7B
        {
            get { return dEtching7B; }
            set { this.dEtching7B = value; }
        }

        private string rEtching7B = "";
        public string REtching7B
        {
            get { return rEtching7B; }
            set { this.rEtching7B = value; }
        }

        private string aEtching7 = "";
        public string AEtching7B
        {
            get { return aEtching7; }
            set { this.aEtching7 = value; }
        }

        private bool tM2Data = false;
        public bool TM2Data
        {
            get { return tM2Data; }
            set { this.tM2Data = value; }
        }

        private int? projectNo = 0;
        public int? ProjectNo
        {
            get { return projectNo; }
            set { this.projectNo = value; }
        }

        private string drawingSetNo = "";
        public string DrawingSetNo
        {
            get { return drawingSetNo; }
            set { this.drawingSetNo = value; }
        }

        private string shipToContact = "";
        public string ShipToContact
        {
            get { return shipToContact; }
            set { this.shipToContact = value; }
        }

        private string aEtching8 = "";
        public string AEtching8
        {
            get { return aEtching8; }
            set { this.aEtching8 = value; }
        }

        private string aEtching9 = "";
        public string AEtching9
        {
            get { return aEtching9; }
            set { this.aEtching9 = value; }
        }

        private string aEtching10 = "";
        public string AEtching10
        {
            get { return aEtching10; }
            set { this.aEtching10 = value; }
        }

        private string shipToTaxRegNo = "";
        public string ShipToTaxRegNo
        {
            get { return shipToTaxRegNo; }
            set { this.shipToTaxRegNo = value; }
        }

        private string soldToTaxRegNo = "";
        public string SoldToTaxRegNo
        {
            get { return soldToTaxRegNo; }
            set { this.soldToTaxRegNo = value; }
        }

        private string userTaxRegNo = "";
        public string UserTaxRegNo
        {
            get { return userTaxRegNo; }
            set { this.userTaxRegNo = value; }
        }

        private string drawingPO = "";
        public string DrawingPO
        {
            get { return drawingPO; }
            set { this.drawingPO = value; }
        }

        private string requested = "";
        public string Requested
        {
            get { return requested; }
            set { this.requested = value; }
        }

        private string email6 = "";
        public string Email6
        {
            get { return email6; }
            set { this.email6 = value; }
        }

        private string email6Name = "";
        public string Email6Name
        {
            get { return email6Name; }
            set { this.email6Name = value; }
        }

        private string email7 = "";
        public string Email7
        {
            get { return email7; }
            set { this.email7 = value; }
        }

        private string email7Name = "";
        public string Email7Name
        {
            get { return email7Name; }
            set { this.email7Name = value; }
        }

        private string email8 = "";
        public string Email8
        {
            get { return email8; }
            set { this.email8 = value; }
        }

        private string email8Name = "";
        public string Email8Name
        {
            get { return email8Name; }
            set { this.email8Name = value; }
        }

        private string shipperQuoteNo = "";
        public string ShipperQuoteNo
        {
            get { return shipperQuoteNo; }
            set { this.shipperQuoteNo = value; }
        }

        private int? refWO = 0;
        public int? RefWO
        {
            get { return refWO; }
            set { this.refWO = value; }
        }

        private char postedtoGPASYN = ' ';
        public char PostedtoGPASYN
        {
            get { return postedtoGPASYN; }
            set { this.postedtoGPASYN = value; }
        }

        #endregion
        private NAT01Context nat01Context;
        private QuoteHeader quoteHeader = new QuoteHeader();
        private List<QuoteDetails> quoteDetails = new List<QuoteDetails>();
        public int LineItemCount { get; set; }
        public NAT01Context Nat01Context { get => nat01Context; set => nat01Context = value; }

        public Dictionary<int, string> lineItems = new Dictionary<int, string>();


        public Quote() { }

        /// <summary>
        /// Instance of a quote, including all details about the quote
        /// </summary>
        /// <param name="quoteNumber"></param>
        /// <param name="quoteRevNo"></param>
        public Quote(int quoteNumber, short quoteRevNo)
        {
            try
            {
                this.quoteNumber = quoteNumber;
                this.quoteRevNo = quoteRevNo;
                nat01Context = new NAT01Context();
                quoteHeader = nat01Context.QuoteHeader.Where(q => q.QuoteNo == quoteNumber && q.QuoteRevNo == quoteRevNo).FirstOrDefault();
                quoteDetails = nat01Context.QuoteDetails.Where(q => q.QuoteNo == quoteNumber && q.Revision == quoteRevNo).ToList();


                LineItemCount = quoteDetails.Count;
                foreach (QuoteDetails row in quoteDetails)
                {
                    if (row.DetailTypeId.Trim() != "")
                    {
                        lineItems.Add(row.LineNumber, row.DetailTypeId.Trim());
                    }

                }
                SetInfo(quoteHeader);
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("Quote.cs -> QuoteNo: " + quoteNumber + "-" + quoteRevNo, ex.Message, null);
            }
        }

        public void SetInfo(QuoteHeader quoteHeader)
        {
            customerNo = string.IsNullOrEmpty(quoteHeader.CustomerNo) ? "" : quoteHeader.CustomerNo.Trim();
            shipToAccountNo = string.IsNullOrEmpty(quoteHeader.ShipToAccountNo) ? "" : quoteHeader.ShipToAccountNo.Trim();
            shipToNo = string.IsNullOrEmpty(quoteHeader.ShipToNo) ? "" : quoteHeader.ShipToNo.Trim();
            lastLineNo = quoteHeader.LastLineNo;
            lastNoteNo = quoteHeader.LastNoteNo;
            quoteDate = quoteHeader.QuoteDate;
            quoteMarkdown = quoteHeader.QuoteMarkdown;
            quoteSubtotal = quoteHeader.QuoteSubtotal;
            quoteFreightCharge = quoteHeader.QuoteFreightCharge;
            orderTax = quoteHeader.OrderTax;
            quoteTotal = quoteHeader.QuoteTotal;
            reference = string.IsNullOrEmpty(quoteHeader.Reference) ? "" : quoteHeader.Reference.Trim();
            pOExtension = string.IsNullOrEmpty(quoteHeader.Poextension) ? "" : quoteHeader.Poextension.Trim();
            shippedVia = string.IsNullOrEmpty(quoteHeader.ShippedVia) ? "" : quoteHeader.ShippedVia.Trim();
            quoteRepID = string.IsNullOrEmpty(quoteHeader.QuoteRepId) ? "" : quoteHeader.QuoteRepId.Trim();
            termsID = string.IsNullOrEmpty(quoteHeader.TermsId) ? "" : quoteHeader.TermsId.Trim();
            char.TryParse(quoteHeader.FreightDescId, out freightDescID);
            char.TryParse(quoteHeader.PrintStatus, out printStatus);
            char.TryParse(quoteHeader.RevisedYn, out revisedYN);
            char.TryParse(quoteHeader.ShippedYn, out shippedYN);
            note1 = string.IsNullOrEmpty(quoteHeader.Note1) ? "" : quoteHeader.Note1.Trim();
            note2 = string.IsNullOrEmpty(quoteHeader.Note2) ? "" : quoteHeader.Note2.Trim();
            shiptoName = string.IsNullOrEmpty(quoteHeader.ShiptoName) ? "" : quoteHeader.ShiptoName.Trim();
            shiptoAddr1 = string.IsNullOrEmpty(quoteHeader.ShiptoAddr1) ? "" : quoteHeader.ShiptoAddr1.Trim();
            shiptoAddr2 = string.IsNullOrEmpty(quoteHeader.ShiptoAddr2) ? "" : quoteHeader.ShiptoAddr2.Trim();
            shiptoAddr3 = string.IsNullOrEmpty(quoteHeader.ShiptoAddr3) ? "" : quoteHeader.ShiptoAddr3.Trim();
            shiptoCity = string.IsNullOrEmpty(quoteHeader.ShiptoCity) ? "" : quoteHeader.ShiptoCity.Trim();
            shiptoState = string.IsNullOrEmpty(quoteHeader.ShiptoState) ? "" : quoteHeader.ShiptoState.Trim();
            shiptoZip = string.IsNullOrEmpty(quoteHeader.ShiptoZip) ? "" : quoteHeader.ShiptoZip.Trim();
            shiptoCountry = string.IsNullOrEmpty(quoteHeader.ShiptoCountry) ? "" : quoteHeader.ShiptoCountry.Trim();
            shipToPhone = string.IsNullOrEmpty(quoteHeader.ShipToPhone) ? "" : quoteHeader.ShipToPhone.Trim();
            requestedShipDate = quoteHeader.RequestedShipDate is null ? DateTime.MinValue : quoteHeader.RequestedShipDate;
            contactPerson = string.IsNullOrEmpty(quoteHeader.ContactPerson) ? "" : quoteHeader.ContactPerson.Trim();
            shipToFax = string.IsNullOrEmpty(quoteHeader.ShipToFax) ? "" : quoteHeader.ShipToFax.Trim();
            lastWONoteNo = quoteHeader.LastWonoteNo;
            char.TryParse(quoteHeader.PrintQuoteStatus, out printQuoteStatus);
            char.TryParse(quoteHeader.ConvertedtoOrder, out convertedToOrder);
            char.TryParse(quoteHeader.PrintPkgSlipStat, out printPkgSlipStat);
            char.TryParse(quoteHeader.PrintMiniWorkOrd, out printMiniWorkOrd);
            char.TryParse(quoteHeader.PrintMisc1, out print_Misc1);
            char.TryParse(quoteHeader.PrintMisc2, out print_Misc2);
            char.TryParse(quoteHeader.PrintMisc3, out print_Misc3);
            char.TryParse(quoteHeader.PrintMisc4, out print_Misc4);
            char.TryParse(quoteHeader.PrintMisc5, out print_Misc5);
            productName = string.IsNullOrEmpty(quoteHeader.ProductName) ? "" : quoteHeader.ProductName.Trim();
            hobRequired = string.IsNullOrEmpty(quoteHeader.HobRequired) ? "" : quoteHeader.HobRequired.Trim();
            QuoteRevNo = quoteHeader.QuoteRevNo;
            char.TryParse(quoteHeader.WorkOrderType, out workOrderType);
            engineeringNote1 = string.IsNullOrEmpty(quoteHeader.EngineeringNote1) ? "" : quoteHeader.EngineeringNote1.Trim();
            engineeringNote2 = string.IsNullOrEmpty(quoteHeader.EngineeringNote2) ? "" : quoteHeader.EngineeringNote2.Trim();
            miscNote = string.IsNullOrEmpty(quoteHeader.MiscNote) ? "" : quoteHeader.MiscNote.Trim();
            etchingNote = string.IsNullOrEmpty(quoteHeader.EtchingNote) ? "" : quoteHeader.EtchingNote.Trim();
            shippingNote = string.IsNullOrEmpty(quoteHeader.ShippingNote) ? "" : quoteHeader.ShippingNote.Trim();
            inspectionNote = string.IsNullOrEmpty(quoteHeader.InspectionNote) ? "" : quoteHeader.InspectionNote.Trim();
            orderNo = quoteHeader.OrderNo;
            shipment = string.IsNullOrEmpty(quoteHeader.Shipment) ? "" : quoteHeader.Shipment.Trim();
            billToName = string.IsNullOrEmpty(quoteHeader.BillToName) ? "" : quoteHeader.BillToName.Trim();
            billToAddr1 = string.IsNullOrEmpty(quoteHeader.BillToAddr1) ? "" : quoteHeader.BillToAddr1.Trim();
            billToAddr2 = string.IsNullOrEmpty(quoteHeader.BillToAddr2) ? "" : quoteHeader.BillToAddr2.Trim();
            billToAddr3 = string.IsNullOrEmpty(quoteHeader.BillToAddr3) ? "" : quoteHeader.BillToAddr3.Trim();
            billToCity = string.IsNullOrEmpty(quoteHeader.BillToCity) ? "" : quoteHeader.BillToCity.Trim();
            billToState = string.IsNullOrEmpty(quoteHeader.BillToState) ? "" : quoteHeader.BillToState.Trim();
            billToZip = string.IsNullOrEmpty(quoteHeader.BillToZip) ? "" : quoteHeader.BillToZip.Trim();
            billToCountry = string.IsNullOrEmpty(quoteHeader.BillToCountry) ? "" : quoteHeader.BillToCountry.Trim();
            pONumber = string.IsNullOrEmpty(quoteHeader.Ponumber) ? "" : quoteHeader.Ponumber.Trim();
            internationalID = string.IsNullOrEmpty(quoteHeader.InternationalId) ? "" : quoteHeader.InternationalId.Trim();
            email = string.IsNullOrEmpty(quoteHeader.Email) ? "" : quoteHeader.Email.Trim();
            emailName = string.IsNullOrEmpty(quoteHeader.EmailName) ? "" : quoteHeader.EmailName.Trim();
            email2 = string.IsNullOrEmpty(quoteHeader.Email2) ? "" : quoteHeader.Email2.Trim();
            email2Name = string.IsNullOrEmpty(quoteHeader.Email2Name) ? "" : quoteHeader.Email2Name.Trim();
            email3 = string.IsNullOrEmpty(quoteHeader.Email3) ? "" : quoteHeader.Email3.Trim();
            email3Name = string.IsNullOrEmpty(quoteHeader.Email3Name) ? "" : quoteHeader.Email3Name.Trim();
            email4 = string.IsNullOrEmpty(quoteHeader.Email4) ? "" : quoteHeader.Email4.Trim();
            email4Name = string.IsNullOrEmpty(quoteHeader.Email4Name) ? "" : quoteHeader.Email4Name.Trim();
            email5 = string.IsNullOrEmpty(quoteHeader.Email5) ? "" : quoteHeader.Email5.Trim();
            email5Name = string.IsNullOrEmpty(quoteHeader.Email5Name) ? "" : quoteHeader.Email5Name.Trim();
            char.TryParse(quoteHeader.RushYorN, out rushYorN);
            rushNatoliContact = string.IsNullOrEmpty(quoteHeader.RushNatoliContact) ? "" : quoteHeader.RushNatoliContact.Trim();
            char.TryParse(quoteHeader.UnitOfMeasure, out unitOfMeasure);
            char.TryParse(quoteHeader.Itnrequired, out iTNRequired);
            commInvType = string.IsNullOrEmpty(quoteHeader.CommInvType) ? "" : quoteHeader.CommInvType.Trim();
            incoterms = string.IsNullOrEmpty(quoteHeader.Incoterms) ? "" : quoteHeader.Incoterms.Trim();
            char.TryParse(quoteHeader.ShippingBillMethod, out shippingBillMethod);
            shippingAcctNo = string.IsNullOrEmpty(quoteHeader.ShippingAcctNo) ? "" : quoteHeader.ShippingAcctNo.Trim();
            dutiesTaxesBilling = string.IsNullOrEmpty(quoteHeader.DutiesTaxesBilling) ? "" : quoteHeader.DutiesTaxesBilling.Trim();
            dutiesTaxesAcctNo = string.IsNullOrEmpty(quoteHeader.DutiesTaxesAcctNo) ? "" : quoteHeader.DutiesTaxesAcctNo.Trim();
            exportReason = string.IsNullOrEmpty(quoteHeader.ExportReason) ? "" : quoteHeader.ExportReason.Trim();
            logisticsContact = string.IsNullOrEmpty(quoteHeader.LogisticsContact) ? "" : quoteHeader.LogisticsContact.Trim();
            logisticsEmail = string.IsNullOrEmpty(quoteHeader.LogisticsEmail) ? "" : quoteHeader.LogisticsEmail.Trim();
            char.TryParse(quoteHeader.InternationalYorN, out internationalYorN);
            char.TryParse(quoteHeader.Pricing, out pricing);
            char.TryParse(quoteHeader.NewCustomer, out newCustomer);
            char.TryParse(quoteHeader.ExportedToEng, out exportedToEng);
            logisticsPhoneNumber = string.IsNullOrEmpty(quoteHeader.LogisticsPhoneNo) ? "" : quoteHeader.LogisticsPhoneNo.Trim();
            iTNNo = string.IsNullOrEmpty(quoteHeader.Itnno) ? "" : quoteHeader.Itnno.Trim();
            currencyID = string.IsNullOrEmpty(quoteHeader.CurrencyId) ? "" : quoteHeader.CurrencyId.Trim();
            exchangeRate = quoteHeader.ExchangeRate;
            uEtching1 = string.IsNullOrEmpty(quoteHeader.Uetching1) ? "" : quoteHeader.Uetching1.Trim();
            uEtching2 = string.IsNullOrEmpty(quoteHeader.Uetching2) ? "" : quoteHeader.Uetching2.Trim();
            uEtching3 = string.IsNullOrEmpty(quoteHeader.Uetching3) ? "" : quoteHeader.Uetching3.Trim();
            uEtching4 = string.IsNullOrEmpty(quoteHeader.Uetching4) ? "" : quoteHeader.Uetching4.Trim();
            uEtching5 = string.IsNullOrEmpty(quoteHeader.Uetching5) ? "" : quoteHeader.Uetching5.Trim();
            uEtching6 = string.IsNullOrEmpty(quoteHeader.Uetching6) ? "" : quoteHeader.Uetching6.Trim();
            lEtching1 = string.IsNullOrEmpty(quoteHeader.Letching1) ? "" : quoteHeader.Letching1.Trim();
            lEtching2 = string.IsNullOrEmpty(quoteHeader.Letching2) ? "" : quoteHeader.Letching2.Trim();
            lEtching3 = string.IsNullOrEmpty(quoteHeader.Letching3) ? "" : quoteHeader.Letching3.Trim();
            lEtching4 = string.IsNullOrEmpty(quoteHeader.Letching4) ? "" : quoteHeader.Letching4.Trim();
            lEtching5 = string.IsNullOrEmpty(quoteHeader.Letching5) ? "" : quoteHeader.Letching5.Trim();
            lEtching6 = string.IsNullOrEmpty(quoteHeader.Letching6) ? "" : quoteHeader.Letching6.Trim();
            dEtching1 = string.IsNullOrEmpty(quoteHeader.Detching1) ? "" : quoteHeader.Detching1.Trim();
            dEtching2 = string.IsNullOrEmpty(quoteHeader.Detching2) ? "" : quoteHeader.Detching2.Trim();
            dEtching3 = string.IsNullOrEmpty(quoteHeader.Detching3) ? "" : quoteHeader.Detching3.Trim();
            dEtching4 = string.IsNullOrEmpty(quoteHeader.Detching4) ? "" : quoteHeader.Detching4.Trim();
            dEtching5 = string.IsNullOrEmpty(quoteHeader.Detching5) ? "" : quoteHeader.Detching5.Trim();
            dEtching6 = string.IsNullOrEmpty(quoteHeader.Detching6) ? "" : quoteHeader.Detching6.Trim();
            rEtching1 = string.IsNullOrEmpty(quoteHeader.Retching1) ? "" : quoteHeader.Retching1.Trim();
            rEtching2 = string.IsNullOrEmpty(quoteHeader.Retching2) ? "" : quoteHeader.Retching2.Trim();
            rEtching3 = string.IsNullOrEmpty(quoteHeader.Retching3) ? "" : quoteHeader.Retching3.Trim();
            rEtching4 = string.IsNullOrEmpty(quoteHeader.Retching4) ? "" : quoteHeader.Retching4.Trim();
            rEtching5 = string.IsNullOrEmpty(quoteHeader.Retching5) ? "" : quoteHeader.Retching5.Trim();
            rEtching6 = string.IsNullOrEmpty(quoteHeader.Retching6) ? "" : quoteHeader.Retching6.Trim();
            userAcctNo = string.IsNullOrEmpty(quoteHeader.UserAcctNo) ? "" : quoteHeader.UserAcctNo.Trim();
            userLocNo = string.IsNullOrEmpty(quoteHeader.UserLocNo) ? "" : quoteHeader.UserLocNo.Trim();
            uEtching1B = string.IsNullOrEmpty(quoteHeader.Uetching1B) ? "" : quoteHeader.Uetching1B.Trim();
            uEtching2B = string.IsNullOrEmpty(quoteHeader.Uetching2B) ? "" : quoteHeader.Uetching2B.Trim();
            uEtching3B = string.IsNullOrEmpty(quoteHeader.Uetching3B) ? "" : quoteHeader.Uetching3B.Trim();
            uEtching4B = string.IsNullOrEmpty(quoteHeader.Uetching4B) ? "" : quoteHeader.Uetching4B.Trim();
            uEtching5B = string.IsNullOrEmpty(quoteHeader.Uetching5B) ? "" : quoteHeader.Uetching5B.Trim();
            uEtching6B = string.IsNullOrEmpty(quoteHeader.Uetching6B) ? "" : quoteHeader.Uetching6B.Trim();
            lEtching1B = string.IsNullOrEmpty(quoteHeader.Letching1B) ? "" : quoteHeader.Letching1B.Trim();
            lEtching2B = string.IsNullOrEmpty(quoteHeader.Letching2B) ? "" : quoteHeader.Letching2B.Trim();
            lEtching3B = string.IsNullOrEmpty(quoteHeader.Letching3B) ? "" : quoteHeader.Letching3B.Trim();
            lEtching4B = string.IsNullOrEmpty(quoteHeader.Letching4B) ? "" : quoteHeader.Letching4B.Trim();
            lEtching5B = string.IsNullOrEmpty(quoteHeader.Letching5B) ? "" : quoteHeader.Letching5B.Trim();
            lEtching6B = string.IsNullOrEmpty(quoteHeader.Letching6B) ? "" : quoteHeader.Letching6B.Trim();
            dEtching1B = string.IsNullOrEmpty(quoteHeader.Detching1B) ? "" : quoteHeader.Detching1B.Trim();
            dEtching2B = string.IsNullOrEmpty(quoteHeader.Detching2B) ? "" : quoteHeader.Detching2B.Trim();
            dEtching3B = string.IsNullOrEmpty(quoteHeader.Detching3B) ? "" : quoteHeader.Detching3B.Trim();
            dEtching4B = string.IsNullOrEmpty(quoteHeader.Detching4B) ? "" : quoteHeader.Detching4B.Trim();
            dEtching5B = string.IsNullOrEmpty(quoteHeader.Detching5B) ? "" : quoteHeader.Detching5B.Trim();
            dEtching6B = string.IsNullOrEmpty(quoteHeader.Detching6B) ? "" : quoteHeader.Detching6B.Trim();
            rEtching1B = string.IsNullOrEmpty(quoteHeader.Retching1B) ? "" : quoteHeader.Retching1B.Trim();
            rEtching2B = string.IsNullOrEmpty(quoteHeader.Retching2B) ? "" : quoteHeader.Retching2B.Trim();
            rEtching3B = string.IsNullOrEmpty(quoteHeader.Retching3B) ? "" : quoteHeader.Retching3B.Trim();
            rEtching4B = string.IsNullOrEmpty(quoteHeader.Retching4B) ? "" : quoteHeader.Retching4B.Trim();
            rEtching5B = string.IsNullOrEmpty(quoteHeader.Retching5B) ? "" : quoteHeader.Retching5B.Trim();
            rEtching6B = string.IsNullOrEmpty(quoteHeader.Retching6B) ? "" : quoteHeader.Retching6B.Trim();
            aEtching1 = string.IsNullOrEmpty(quoteHeader.Aetching1) ? "" : quoteHeader.Aetching1.Trim();
            aEtching2 = string.IsNullOrEmpty(quoteHeader.Aetching2) ? "" : quoteHeader.Aetching2.Trim();
            aEtching3 = string.IsNullOrEmpty(quoteHeader.Aetching3) ? "" : quoteHeader.Aetching3.Trim();
            aEtching4 = string.IsNullOrEmpty(quoteHeader.Aetching4) ? "" : quoteHeader.Aetching4.Trim();
            aEtching5 = string.IsNullOrEmpty(quoteHeader.Aetching5) ? "" : quoteHeader.Aetching5.Trim();
            aEtching6 = string.IsNullOrEmpty(quoteHeader.Aetching6) ? "" : quoteHeader.Aetching6.Trim();
            origination = string.IsNullOrEmpty(quoteHeader.Origination) ? "" : quoteHeader.Origination.Trim();
            uEtching7 = string.IsNullOrEmpty(quoteHeader.Uetching7) ? "" : quoteHeader.Uetching7.Trim();
            lEtching7 = string.IsNullOrEmpty(quoteHeader.Letching7) ? "" : quoteHeader.Letching7.Trim();
            dEtching7 = string.IsNullOrEmpty(quoteHeader.Detching7) ? "" : quoteHeader.Detching7.Trim();
            rEtching7 = string.IsNullOrEmpty(quoteHeader.Retching7) ? "" : quoteHeader.Retching7.Trim();
            uEtching7B = string.IsNullOrEmpty(quoteHeader.Uetching7B) ? "" : quoteHeader.Uetching7B.Trim();
            lEtching7B = string.IsNullOrEmpty(quoteHeader.Letching7B) ? "" : quoteHeader.Letching7B.Trim();
            dEtching7B = string.IsNullOrEmpty(quoteHeader.Detching7B) ? "" : quoteHeader.Detching7B.Trim();
            rEtching7B = string.IsNullOrEmpty(quoteHeader.Retching7B) ? "" : quoteHeader.Retching7B.Trim();
            aEtching7 = string.IsNullOrEmpty(quoteHeader.Aetching7) ? "" : quoteHeader.Aetching7.Trim();
            bool.TryParse(quoteHeader.Tm2data.ToString(), out tM2Data);
            projectNo = quoteHeader.ProjectNo;
            drawingSetNo = string.IsNullOrEmpty(quoteHeader.DrawingSetNo) ? "" : quoteHeader.DrawingSetNo.Trim();
            shipToContact = string.IsNullOrEmpty(quoteHeader.ShipToContact) ? "" : quoteHeader.ShipToContact.Trim();
            aEtching8 = string.IsNullOrEmpty(quoteHeader.Aetching8) ? "" : quoteHeader.Aetching8.Trim();
            aEtching9 = string.IsNullOrEmpty(quoteHeader.Aetching9) ? "" : quoteHeader.Aetching9.Trim();
            aEtching10 = string.IsNullOrEmpty(quoteHeader.Aetching10) ? "" : quoteHeader.Aetching10.Trim();
            shipToTaxRegNo = string.IsNullOrEmpty(quoteHeader.ShipToTaxRegNo) ? "" : quoteHeader.ShipToTaxRegNo.Trim();
            soldToTaxRegNo = string.IsNullOrEmpty(quoteHeader.SoldToTaxRegNo) ? "" : quoteHeader.SoldToTaxRegNo.Trim();
            userTaxRegNo = string.IsNullOrEmpty(quoteHeader.UserTaxRegNo) ? "" : quoteHeader.UserTaxRegNo.Trim();
            drawingPO = string.IsNullOrEmpty(quoteHeader.DrawingPo) ? "" : quoteHeader.DrawingPo.Trim();
            requested = string.IsNullOrEmpty(quoteHeader.Requested) ? "" : quoteHeader.Requested.Trim();
            email6 = string.IsNullOrEmpty(quoteHeader.Email6) ? "" : quoteHeader.Email6.Trim();
            email6Name = string.IsNullOrEmpty(quoteHeader.Email6Name) ? "" : quoteHeader.Email6Name.Trim();
            email7 = string.IsNullOrEmpty(quoteHeader.Email7) ? "" : quoteHeader.Email7.Trim();
            email7Name = string.IsNullOrEmpty(quoteHeader.Email7Name) ? "" : quoteHeader.Email7Name.Trim();
            email8 = string.IsNullOrEmpty(quoteHeader.Email8) ? "" : quoteHeader.Email8.Trim();
            email8Name = string.IsNullOrEmpty(quoteHeader.Email8Name) ? "" : quoteHeader.Email8Name.Trim();
            shipperQuoteNo = string.IsNullOrEmpty(quoteHeader.ShipperQuoteNo) ? "" : quoteHeader.ShipperQuoteNo.Trim();
            refWO = quoteHeader.RefWo;
            char.TryParse(quoteHeader.PostedtoGpasyn, out postedtoGPASYN);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Quote()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
