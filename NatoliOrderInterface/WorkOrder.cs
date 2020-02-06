using Microsoft.EntityFrameworkCore;
using NatoliOrderInterface.Models;
using NatoliOrderInterface.Models.NAT01;
using NatoliOrderInterface.Models.NEC;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace NatoliOrderInterface
{
    public class WorkOrder
    {
        #region SettersAndGetters
        private int orderNumber;
        public int OrderNumber
        {
            get { return orderNumber; }
            set { this.orderNumber = value; }
        }

        private string customerNumber;
        public string CustomerNumber
        {
            get { return customerNumber; }
            set { this.customerNumber = value; }
        }

        private string newCustomer;
        public string NewCustomer
        {
            get { return newCustomer; }
            set { this.newCustomer = value; }
        }

        private string soldToCustomerName;
        public string SoldToCustomerName
        {
            get { return soldToCustomerName; }
            set { this.soldToCustomerName = value; }
        }

        private DateTime orderDate;
        public DateTime OrderDate
        {
            get { return orderDate; }
            set { this.orderDate = value; }
        }

        private string poNumber;
        public string PoNumber
        {
            get { return poNumber; }
            set { this.poNumber = value; }
        }

        private string shipToCustomerName;
        public string ShipToCustomerName
        {
            get { return shipToCustomerName; }
            set { this.shipToCustomerName = value; }
        }

        private DateTime shipDate;
        public DateTime ShipDate
        {
            get { return shipDate; }
            set { this.shipDate = value; }
        }

        private string endUserName;
        public string EndUserName
        {
            get { return endUserName; }
            set { this.endUserName = value; }
        }

        private int? referenceOrder;
        public int? ReferenceOrder
        {
            get { return referenceOrder; }
            set { this.referenceOrder = value; }
        }

        private string engineeringNotes;
        public string EngineeringNotes
        {
            get { return engineeringNotes; }
            set { this.engineeringNotes = value; }
        }

        private string csr;
        public string Csr
        {
            get { return csr; }
            set { this.csr = value; }
        }

        private string accountNumber;
        public string AccountNumber
        {
            get { return accountNumber; }
            set { this.accountNumber = value; }
        }

        private string terms;
        public string Terms
        {
            get { return terms; }
            set { this.terms = value; }
        }

        private string soldBy;
        public string SoldBy
        {
            get { return soldBy; }
            set { this.soldBy = value; }
        }

        private string shippedVia;
        public string ShippedVia
        {
            get { return shippedVia; }
            set { this.shippedVia = value; }
        }

        private string quoteNumber;
        public string QuoteNumber
        {
            get { return quoteNumber; }
            set { this.quoteNumber = value; }
        }

        private string reference;
        public string Reference
        {
            get { return reference; }
            set { this.reference = value; }
        }

        private int? projectNumber;
        public int? ProjectNumber
        {
            get { return projectNumber; }
            set { this.projectNumber = value; }
        }

        private string hobRequired;
        public string HobRequired
        {
            get { return hobRequired; }
            set { this.hobRequired = value; }
        }

        private string userNumber;
        public string UserNumber
        {
            get { return userNumber; }
            set { this.userNumber = value; }
        }

        private string userLoc;
        public string UserLoc
        {
            get { return userLoc; }
            set { this.userLoc = value; }
        }

        private string productName;
        public string ProductName
        {
            get { return productName; }
            set { this.productName = value; }
        }

        private string rushYOrN;
        public string RushYOrN
        {
            get { return rushYOrN; }
            set { this.rushYOrN = value; }
        }

        private string paidRushFee;
        public string PaidRushFee
        {
            get { return paidRushFee; }
            set { this.paidRushFee = value; }
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

        private string inspectionNote = "";
        public string InspectionNote
        {
            get { return inspectionNote; }
            set { this.inspectionNote = value; }
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

        private string shipWithWONo = "";
        public string ShipWithWONo
        {
            get { return shipWithWONo; }
            set { this.shipWithWONo = value; }
        }


        private List<OrderLineItem> lineItemsList;
        public List<OrderLineItem> LineItemsList
        {
            get { return lineItemsList; }
            set { this.lineItemsList = value; }
        }
        #endregion

        public int LineItemCount { get; set; }
        public Dictionary<int, string> lineItems = new Dictionary<int, string>();
        public bool Finished { get; set; }
        public bool CanRunOnAutocell { get; set; }
        private List<int> lineItemsToScan = new List<int>();
        private StackPanel stackPanelTemp;
        private Window parent;

        public WorkOrder() { }

        /// <summary>
        /// Instance of a work order, complete with all details about the work order
        /// </summary>
        /// <param name="orderNumber"></param>
        public WorkOrder(int orderNumber, Window parent)
        {
            try
            {
                OrderNumber = orderNumber;
                Finished = false;
                CanRunOnAutocell = false;
                this.parent = parent;

                // nat01context.OrderDetails.Where(o => o.OrderNo == OrderNumber).Load();
                using (var context = new NAT02Context())
                {
                    Finished = context.EoiOrdersMarkedForChecking.Any(o => o.OrderNo == OrderNumber);
                    context.Dispose();
                }
                using (var nat01context = new NAT01Context())
                {
                    LineItemCount = nat01context.OrderDetails.Where(o => o.OrderNo == OrderNumber * 100).Count();
                    lineItems = nat01context.OrderDetails.Where(o => o.OrderNo == OrderNumber * 100).ToDictionary(kvp => (int)kvp.LineNumber, kvp => kvp.DetailTypeId.Trim());
                    OrderHeader orderHeader = nat01context.OrderHeader.Where(o => o.OrderNo == OrderNumber * 100).FirstOrDefault();
                    List<OrderDetails> orderDetails = nat01context.OrderDetails.Where(o => o.OrderNo == OrderNumber * 100).ToList();
                    string repId = "";
                    string csr = "*NO CSR*";
                    if (nat01context.QuoteHeader.Any(q => q.QuoteNo == orderHeader.QuoteNumber && q.QuoteRevNo == orderHeader.QuoteRevNo))
                    {
                        repId = string.IsNullOrEmpty(nat01context.QuoteHeader.Where(q => q.QuoteNo == orderHeader.QuoteNumber && q.QuoteRevNo == orderHeader.QuoteRevNo).First().QuoteRepId) ? "" : nat01context.QuoteHeader.Where(q => q.QuoteNo == orderHeader.QuoteNumber && q.QuoteRevNo == orderHeader.QuoteRevNo).First().QuoteRepId.Trim();
                        if (nat01context.QuoteRepresentative.Any(qr => qr.RepId == repId))
                        {
                            csr = string.IsNullOrEmpty(nat01context.QuoteRepresentative.Where(qr => qr.RepId == repId).First().Name) ? "*NO CSR*" : nat01context.QuoteRepresentative.Where(qr => qr.RepId == repId).First().Name.Trim();
                        }
                    }
                    string customerName; string endUserName;
                    using (var ctx = new NECContext())
                    {
                        customerName = ctx.Rm00101.Where(c => c.Custnmbr == orderHeader.CustomerNo).FirstOrDefault().Custname;
                        endUserName = ctx.Rm00101.Where(c => c.Custnmbr == orderHeader.UserAcctNo).FirstOrDefault().Custname;
                        ctx.Dispose();
                    }
                    nat01context.Dispose();
                    SetInfo(orderHeader, csr, customerName, endUserName);
                }
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("WorkOrder.cs -> OrderNumber: " + OrderNumber , ex.Message, null);
            }
        }

        public void SetInfo(OrderHeader orderHeader, string csr, string customerName, string endUserName)
        {
            customerNumber = string.IsNullOrEmpty(orderHeader.CustomerNo) ? "" : orderHeader.CustomerNo.Trim();
            newCustomer = string.IsNullOrEmpty(orderHeader.NewCustomer) ? "" : orderHeader.NewCustomer.Trim();
            soldToCustomerName = string.IsNullOrEmpty(customerName) ? "" : customerName.Trim();
            orderDate = (DateTime)orderHeader.OrderDate;
            poNumber = (string.IsNullOrEmpty(orderHeader.Ponumber) ? "" :  orderHeader.Ponumber.Trim()) + ' ' + (string.IsNullOrEmpty(orderHeader.Poextension) ? "" : orderHeader.Poextension.Trim());
            shipToCustomerName = string.IsNullOrEmpty(orderHeader.ShiptoName) ? "" : orderHeader.ShiptoName.Trim();
            shipDate = (DateTime)orderHeader.RequestedShipDateRev;
            if (shipDate.ToShortDateString() != "01/01/1901")
            {
                shipDate = (DateTime)orderHeader.RequestedShipDate;
            }
            this.endUserName = string.IsNullOrEmpty(endUserName) ? "" : endUserName.Trim();
            referenceOrder = orderHeader.RefWo;
            engineeringNotes = orderHeader.EngineeringNote1.Trim() + "\n" + orderHeader.EngineeringNote2.Trim() + "\n" + orderHeader.MiscNote.Trim();
            inspectionNote = string.IsNullOrEmpty(orderHeader.InspectionNote) ? "" : orderHeader.InspectionNote.Trim();
            etchingNote = string.IsNullOrEmpty(orderHeader.EtchingNote) ? "" : orderHeader.EtchingNote.Trim();
            shippingNote = string.IsNullOrEmpty(orderHeader.ShippingNote) ? "" : orderHeader.ShippingNote.Trim();
            shipWithWONo = string.IsNullOrEmpty(orderHeader.ShipWithWono) ? "" : orderHeader.ShipWithWono.Trim();
            this.csr = string.IsNullOrEmpty(csr) ? "" : csr.Trim();
            accountNumber = string.IsNullOrEmpty(orderHeader.ShipToAccountNo) ? "" : orderHeader.ShipToAccountNo.Trim();
            terms = string.IsNullOrEmpty(orderHeader.TermsId) ? "" : orderHeader.TermsId.Trim();
            soldBy = string.IsNullOrEmpty(orderHeader.SalesRepId) ? "" : orderHeader.SalesRepId.Trim();
            shippedVia = string.IsNullOrEmpty(orderHeader.ShippedVia) ? "" : orderHeader.ShippedVia.Trim();
            quoteNumber = orderHeader.QuoteNumber + "-" + orderHeader.QuoteRevNo;
            reference = string.IsNullOrEmpty(orderHeader.Reference) ? "" : orderHeader.Reference.Trim();
            projectNumber = orderHeader.ProjectNo;
            hobRequired = string.IsNullOrEmpty(orderHeader.HobRequired) ? "" : orderHeader.HobRequired.Trim();
            userNumber = string.IsNullOrEmpty(orderHeader.UserAcctNo) ? "" : orderHeader.UserAcctNo.Trim();
            userLoc = string.IsNullOrEmpty(orderHeader.UserLocNo) ? "" : orderHeader.UserLocNo.Trim(); 
            productName = string.IsNullOrEmpty(orderHeader.ProductName) ? "" : orderHeader.ProductName.Trim();
            rushYOrN = string.IsNullOrEmpty(orderHeader.RushYorN) ? "" : orderHeader.RushYorN.Trim();
            paidRushFee = string.IsNullOrEmpty(orderHeader.PaidRushFee) ? "" : orderHeader.PaidRushFee.Trim();

            uEtching1 = string.IsNullOrEmpty(orderHeader.Uetching1) ? "" : orderHeader.Uetching1.Trim();
            uEtching2 = string.IsNullOrEmpty(orderHeader.Uetching2) ? "" : orderHeader.Uetching2.Trim();
            uEtching3 = string.IsNullOrEmpty(orderHeader.Uetching3) ? "" : orderHeader.Uetching3.Trim();
            uEtching4 = string.IsNullOrEmpty(orderHeader.Uetching4) ? "" : orderHeader.Uetching4.Trim();
            uEtching5 = string.IsNullOrEmpty(orderHeader.Uetching5) ? "" : orderHeader.Uetching5.Trim();
            uEtching6 = string.IsNullOrEmpty(orderHeader.Uetching6) ? "" : orderHeader.Uetching6.Trim();
            lEtching1 = string.IsNullOrEmpty(orderHeader.Letching1) ? "" : orderHeader.Letching1.Trim();
            lEtching2 = string.IsNullOrEmpty(orderHeader.Letching2) ? "" : orderHeader.Letching2.Trim();
            lEtching3 = string.IsNullOrEmpty(orderHeader.Letching3) ? "" : orderHeader.Letching3.Trim();
            lEtching4 = string.IsNullOrEmpty(orderHeader.Letching4) ? "" : orderHeader.Letching4.Trim();
            lEtching5 = string.IsNullOrEmpty(orderHeader.Letching5) ? "" : orderHeader.Letching5.Trim();
            lEtching6 = string.IsNullOrEmpty(orderHeader.Letching6) ? "" : orderHeader.Letching6.Trim();
            dEtching1 = string.IsNullOrEmpty(orderHeader.Detching1) ? "" : orderHeader.Detching1.Trim();
            dEtching2 = string.IsNullOrEmpty(orderHeader.Detching2) ? "" : orderHeader.Detching2.Trim();
            dEtching3 = string.IsNullOrEmpty(orderHeader.Detching3) ? "" : orderHeader.Detching3.Trim();
            dEtching4 = string.IsNullOrEmpty(orderHeader.Detching4) ? "" : orderHeader.Detching4.Trim();
            dEtching5 = string.IsNullOrEmpty(orderHeader.Detching5) ? "" : orderHeader.Detching5.Trim();
            dEtching6 = string.IsNullOrEmpty(orderHeader.Detching6) ? "" : orderHeader.Detching6.Trim();
            rEtching1 = string.IsNullOrEmpty(orderHeader.Retching1) ? "" : orderHeader.Retching1.Trim();
            rEtching2 = string.IsNullOrEmpty(orderHeader.Retching2) ? "" : orderHeader.Retching2.Trim();
            rEtching3 = string.IsNullOrEmpty(orderHeader.Retching3) ? "" : orderHeader.Retching3.Trim();
            rEtching4 = string.IsNullOrEmpty(orderHeader.Retching4) ? "" : orderHeader.Retching4.Trim();
            rEtching5 = string.IsNullOrEmpty(orderHeader.Retching5) ? "" : orderHeader.Retching5.Trim();
            rEtching6 = string.IsNullOrEmpty(orderHeader.Retching6) ? "" : orderHeader.Retching6.Trim();
            uEtching1B = string.IsNullOrEmpty(orderHeader.Uetching1B) ? "" : orderHeader.Uetching1B.Trim();
            uEtching2B = string.IsNullOrEmpty(orderHeader.Uetching2B) ? "" : orderHeader.Uetching2B.Trim();
            uEtching3B = string.IsNullOrEmpty(orderHeader.Uetching3B) ? "" : orderHeader.Uetching3B.Trim();
            uEtching4B = string.IsNullOrEmpty(orderHeader.Uetching4B) ? "" : orderHeader.Uetching4B.Trim();
            uEtching5B = string.IsNullOrEmpty(orderHeader.Uetching5B) ? "" : orderHeader.Uetching5B.Trim();
            uEtching6B = string.IsNullOrEmpty(orderHeader.Uetching6B) ? "" : orderHeader.Uetching6B.Trim();
            lEtching1B = string.IsNullOrEmpty(orderHeader.Letching1B) ? "" : orderHeader.Letching1B.Trim();
            lEtching2B = string.IsNullOrEmpty(orderHeader.Letching2B) ? "" : orderHeader.Letching2B.Trim();
            lEtching3B = string.IsNullOrEmpty(orderHeader.Letching3B) ? "" : orderHeader.Letching3B.Trim();
            lEtching4B = string.IsNullOrEmpty(orderHeader.Letching4B) ? "" : orderHeader.Letching4B.Trim();
            lEtching5B = string.IsNullOrEmpty(orderHeader.Letching5B) ? "" : orderHeader.Letching5B.Trim();
            lEtching6B = string.IsNullOrEmpty(orderHeader.Letching6B) ? "" : orderHeader.Letching6B.Trim();
            dEtching1B = string.IsNullOrEmpty(orderHeader.Detching1B) ? "" : orderHeader.Detching1B.Trim();
            dEtching2B = string.IsNullOrEmpty(orderHeader.Detching2B) ? "" : orderHeader.Detching2B.Trim();
            dEtching3B = string.IsNullOrEmpty(orderHeader.Detching3B) ? "" : orderHeader.Detching3B.Trim();
            dEtching4B = string.IsNullOrEmpty(orderHeader.Detching4B) ? "" : orderHeader.Detching4B.Trim();
            dEtching5B = string.IsNullOrEmpty(orderHeader.Detching5B) ? "" : orderHeader.Detching5B.Trim();
            dEtching6B = string.IsNullOrEmpty(orderHeader.Detching6B) ? "" : orderHeader.Detching6B.Trim();
            rEtching1B = string.IsNullOrEmpty(orderHeader.Retching1B) ? "" : orderHeader.Retching1B.Trim();
            rEtching2B = string.IsNullOrEmpty(orderHeader.Retching2B) ? "" : orderHeader.Retching2B.Trim();
            rEtching3B = string.IsNullOrEmpty(orderHeader.Retching3B) ? "" : orderHeader.Retching3B.Trim();
            rEtching4B = string.IsNullOrEmpty(orderHeader.Retching4B) ? "" : orderHeader.Retching4B.Trim();
            rEtching5B = string.IsNullOrEmpty(orderHeader.Retching5B) ? "" : orderHeader.Retching5B.Trim();
            rEtching6B = string.IsNullOrEmpty(orderHeader.Retching6B) ? "" : orderHeader.Retching6B.Trim();
            aEtching1 = string.IsNullOrEmpty(orderHeader.Aetching1) ? "" : orderHeader.Aetching1.Trim();
            aEtching2 = string.IsNullOrEmpty(orderHeader.Aetching2) ? "" : orderHeader.Aetching2.Trim();
            aEtching3 = string.IsNullOrEmpty(orderHeader.Aetching3) ? "" : orderHeader.Aetching3.Trim();
            aEtching4 = string.IsNullOrEmpty(orderHeader.Aetching4) ? "" : orderHeader.Aetching4.Trim();
            aEtching5 = string.IsNullOrEmpty(orderHeader.Aetching5) ? "" : orderHeader.Aetching5.Trim();
            aEtching6 = string.IsNullOrEmpty(orderHeader.Aetching6) ? "" : orderHeader.Aetching6.Trim();
            uEtching7 = string.IsNullOrEmpty(orderHeader.Uetching7) ? "" : orderHeader.Uetching7.Trim();
            lEtching7 = string.IsNullOrEmpty(orderHeader.Letching7) ? "" : orderHeader.Letching7.Trim();
            dEtching7 = string.IsNullOrEmpty(orderHeader.Detching7) ? "" : orderHeader.Detching7.Trim();
            rEtching7 = string.IsNullOrEmpty(orderHeader.Retching7) ? "" : orderHeader.Retching7.Trim();
            uEtching7B = string.IsNullOrEmpty(orderHeader.Uetching7B) ? "" : orderHeader.Uetching7B.Trim();
            lEtching7B = string.IsNullOrEmpty(orderHeader.Letching7B) ? "" : orderHeader.Letching7B.Trim();
            dEtching7B = string.IsNullOrEmpty(orderHeader.Detching7B) ? "" : orderHeader.Detching7B.Trim();
            rEtching7B = string.IsNullOrEmpty(orderHeader.Retching7B) ? "" : orderHeader.Retching7B.Trim();
            aEtching7 = string.IsNullOrEmpty(orderHeader.Aetching7) ? "" : orderHeader.Aetching7.Trim();
            aEtching8 = string.IsNullOrEmpty(orderHeader.Aetching8) ? "" : orderHeader.Aetching8.Trim();
            aEtching9 = string.IsNullOrEmpty(orderHeader.Aetching9) ? "" : orderHeader.Aetching9.Trim();
            aEtching10 = string.IsNullOrEmpty(orderHeader.Aetching10) ? "" : orderHeader.Aetching10.Trim();
        }
        public void GenerateTravellerNumbers()
        {
            using var _nat01context = new NAT01Context();
            IQueryable<OrderDetails> orderDetails = _nat01context.OrderDetails.Where(o => o.OrderNo == OrderNumber * 100);
            try
            {
                foreach (OrderDetails lineItem in orderDetails)
                {
                    lineItem.TravellerNo = "1" + lineItem.LineNumber.ToString("00") + OrderNumber + "00";
                    _nat01context.OrderDetails.Update(lineItem);
                }
                _nat01context.SaveChanges();
            }
            catch (Exception ex)
            {

            }
            finally
            {
                _nat01context.Dispose();
            }
        }
        /// <summary>
        /// Transfer orders using given user object and department to transfer to as a string.
        /// Department MUST be in the format "D000[0]". First scan refers to whether the order
        /// has ever been scanned into the system for the first time.
        /// Returns 0 on sucessful transfer.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="transToDept"></param>
        /// <param name="firstScan"></param>
        /// <returns></returns>
        public int TransferOrder(User user, string transToDept, bool firstScan = false)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user), "User cannot be null");
            if (transToDept is null)
                throw new ArgumentNullException(nameof(transToDept), "User cannot be null");
            if (!transToDept.StartsWith('D'))
                throw new ArgumentException("Department must begin with a 'D'", nameof(transToDept));

            if (transToDept.EndsWith("080")) { GetLineItemsToScan(parent); }

            if (firstScan) { GenerateTravellerNumbers(); }

            if (lineItemsToScan.Any())
            {
                foreach (KeyValuePair<int, string> kvp in lineItems)
                {
                    if (!lineItemsToScan.Contains(kvp.Key)) { continue; }
                    try
                    {
                        string lineType = kvp.Value.Trim();
                        if (lineType != "E" && lineType != "H" && lineType != "MC" && lineType != "RET" && lineType != "T" && lineType != "TM" && lineType != "Z")
                        {
                            string travellerNumber = "1" + kvp.Key.ToString("00") + OrderNumber + "00";
                            BarcodeTransfer(user.EmployeeCode, transToDept, travellerNumber);
                        }
                    }
                    catch
                    {
                        return 1;
                    }
                }
            }
            else
            {
                foreach (KeyValuePair<int, string> kvp in lineItems)
                {
                    try
                    {
                        string lineType = kvp.Value.Trim();
                        if (lineType != "E" && lineType != "H" && lineType != "MC" && lineType != "RET" && lineType != "T" && lineType != "TM" && lineType != "Z")
                        {
                            string travellerNumber = "1" + kvp.Key.ToString("00") + OrderNumber + "00";
                            BarcodeTransfer(user.EmployeeCode, transToDept, travellerNumber);
                        }
                    }
                    catch
                    {
                        return 1;
                    }
                }
            }
            
            return 0;
        }

        private void BarcodeTransfer(string employeeCode, string departmentCode, string travellerNumber)
        {
            SqlConnection con = new SqlConnection();
            SqlCommand cmd = new SqlCommand();
            try
            {
                cmd.Connection = con;
                cmd.CommandText = "spValidateScanTransfer";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                // Terminal ID
                cmd.Parameters.Add("@TerminalID", System.Data.SqlDbType.VarChar);
                cmd.Parameters["@TerminalID"].Size = 50;
                cmd.Parameters["@TerminalID"].Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters["@TerminalID"].Value = Environment.MachineName;

                // NTUserID
                cmd.Parameters.Add("@NTUserID", System.Data.SqlDbType.VarChar);
                cmd.Parameters["@NTUserID"].Size = 35;
                cmd.Parameters["@NTUserID"].Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters["@NTUserID"].Value = Environment.MachineName;

                // ApplicationVersion
                cmd.Parameters.Add("@ApplicationVersion", System.Data.SqlDbType.VarChar);
                cmd.Parameters["@ApplicationVersion"].Size = 25;
                cmd.Parameters["@ApplicationVersion"].Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters["@ApplicationVersion"].Value = "V3.4";

                // Browser Details
                cmd.Parameters.Add("@BrowserDetails", System.Data.SqlDbType.VarChar);
                cmd.Parameters["@BrowserDetails"].Size = 75;
                cmd.Parameters["@BrowserDetails"].Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters["@BrowserDetails"].Value = "";

                // ActionPerformed
                cmd.Parameters.Add("@ActionPerformed", System.Data.SqlDbType.VarChar);
                cmd.Parameters["@ActionPerformed"].Size = 75;
                cmd.Parameters["@ActionPerformed"].Direction = System.Data.ParameterDirection.Output;

                // EmployeeCode
                cmd.Parameters.Add("@EmployeeCode", System.Data.SqlDbType.Char);
                cmd.Parameters["@EmployeeCode"].Size = 7;
                cmd.Parameters["@EmployeeCode"].Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters["@EmployeeCode"].Value = employeeCode;

                // CardCode
                cmd.Parameters.Add("@CardCode", System.Data.SqlDbType.Char);
                cmd.Parameters["@CardCode"].Size = 7;
                cmd.Parameters["@CardCode"].Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters["@CardCode"].Value = "";


                // EmployeeValidationCode
                cmd.Parameters.Add("@EmployeeValidationCode", System.Data.SqlDbType.SmallInt);
                cmd.Parameters["@EmployeeValidationCode"].Direction = System.Data.ParameterDirection.Output;

                // EmployeeValidationText
                cmd.Parameters.Add("@EmployeeValidationText", System.Data.SqlDbType.VarChar);
                cmd.Parameters["@EmployeeValidationText"].Size = 75;
                cmd.Parameters["@EmployeeValidationText"].Direction = System.Data.ParameterDirection.Output;

                // DepartmentCode
                cmd.Parameters.Add("@DepartmentCode", System.Data.SqlDbType.Char);
                cmd.Parameters["@DepartmentCode"].Size = 7;
                cmd.Parameters["@DepartmentCode"].Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters["@DepartmentCode"].Value = departmentCode;

                // DepartmentValidationCode
                cmd.Parameters.Add("@DepartmentValidationCode", System.Data.SqlDbType.SmallInt);
                cmd.Parameters["@DepartmentValidationCode"].Direction = System.Data.ParameterDirection.Output;

                // DepartmentValidationText
                cmd.Parameters.Add("@DepartmentValidationText", System.Data.SqlDbType.VarChar);
                cmd.Parameters["@DepartmentValidationText"].Size = 75;
                cmd.Parameters["@DepartmentValidationText"].Direction = System.Data.ParameterDirection.Output;


                // TravellerNo
                cmd.Parameters.Add("@TravellerNo", System.Data.SqlDbType.VarChar);
                cmd.Parameters["@TravellerNo"].Size = 11;
                cmd.Parameters["@TravellerNo"].Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters["@TravellerNo"].Value = travellerNumber;

                // WorkOrderNumber
                cmd.Parameters.Add("@WorkOrderNumber", System.Data.SqlDbType.VarChar);
                cmd.Parameters["@WorkOrderNumber"].Size = 11;
                cmd.Parameters["@WorkOrderNumber"].Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters["@WorkOrderNumber"].Value = "W" + travellerNumber.Substring(3, 6) + "S0" + travellerNumber.Substring(1, 2);

                // WorkOrderNumberValidated
                cmd.Parameters.Add("@WorkOrderNumberValidationCode", System.Data.SqlDbType.SmallInt);
                cmd.Parameters["@WorkOrderNumberValidationCode"].Direction = System.Data.ParameterDirection.Output;

                // WorkOrderNumberValidationText
                cmd.Parameters.Add("@WorkOrderNumberValidationText", System.Data.SqlDbType.VarChar);
                cmd.Parameters["@WorkOrderNumberValidationText"].Size = 75;
                cmd.Parameters["@WorkOrderNumberValidationText"].Direction = System.Data.ParameterDirection.Output;

                // BatchID
                cmd.Parameters.Add("@BatchID", System.Data.SqlDbType.VarChar);
                cmd.Parameters["@BatchID"].Size = 27;
                cmd.Parameters["@BatchID"].Direction = System.Data.ParameterDirection.InputOutput;
                cmd.Parameters["@BatchID"].Value = "none";


                // SaveValidationCode
                cmd.Parameters.Add("@SaveValidationCode", System.Data.SqlDbType.SmallInt);
                cmd.Parameters["@SaveValidationCode"].Direction = System.Data.ParameterDirection.Output;

                // SaveValidationText
                cmd.Parameters.Add("@SaveValidationText", System.Data.SqlDbType.VarChar);
                cmd.Parameters["@SaveValidationText"].Size = 75;
                cmd.Parameters["@SaveValidationText"].Direction = System.Data.ParameterDirection.Output;

                using (con)
                {
                    con.ConnectionString = "Data Source=" + App.Server + ";Initial Catalog=NATBC;Persist Security Info=True;User ID=" + App.UserID+";Password="+App.Password+"";
                    con.Open();
                    DataTable TransferBatch = new DataTable();
                    TransferBatch.Load(cmd.ExecuteReader());
                    TransferBatch.Dispose();
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                cmd.Dispose();
                con.Close();
            }
        }
        private void LineItemChoices_Closed(object sender, EventArgs e)
        {
            IEnumerable<CheckBox> collection = stackPanelTemp.Children.OfType<CheckBox>().Where(c => c.IsChecked == true);
            foreach (CheckBox checkBox in collection)
            {
                IEnumerable<KeyValuePair<int, string>> lineItem = lineItems.Where(x => int.Parse(checkBox.Tag.ToString()) == x.Key);
                lineItemsToScan.Add(lineItem.ElementAt(0).Key);
            }
        }
        private void GetLineItemsToScan(Window owner)
        {
            Window LineItemChoices = new Window();
            LineItemChoices.Owner = owner;
            LineItemChoices.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            LineItemChoices.Width = 250;
            //LineItemChoices.Height = 200;
            Grid grid = new Grid();
            StackPanel stackPanel = new StackPanel();
            stackPanel.Width = 225;
            stackPanel.Margin = new Thickness(0, 15, 0, 0);
            Label label = new Label();
            label.Content = "Check the items you want to send back.";
            label.Width = 225;
            label.HorizontalContentAlignment = HorizontalAlignment.Center;
            stackPanelTemp = stackPanel;
            stackPanel.Children.Add(label);
            grid.Children.Add(stackPanel);
            LineItemChoices.Content = grid;
            LineItemChoices.SizeToContent = SizeToContent.Height;

            foreach (KeyValuePair<int, string> kvp in lineItems)
            {
                string lineType = kvp.Value.Trim();
                if (lineType != "E" && lineType != "H" && lineType != "MC" && lineType != "RET" && lineType != "T" && lineType != "TM" && lineType != "Z")
                {
                    using var context = new NAT01Context();
                    string lineDesc = context.OedetailType.Where(x => lineType == x.TypeId.Trim()).FirstOrDefault().ShortDesc.Trim();
                    CheckBox checkBox = new CheckBox
                    {
                        Name = lineType + "CheckBox",
                        Content = lineDesc,
                        IsChecked = true,
                        Tag = kvp.Key,
                        Margin = new Thickness(0,0,0,15)
                    };
                    stackPanel.Children.Add(checkBox);
                    context.Dispose();
                }
            }

            LineItemChoices.Closed += LineItemChoices_Closed;
            LineItemChoices.ShowDialog();
        }
    }
}
