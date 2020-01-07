using Microsoft.EntityFrameworkCore;
using NatoliOrderInterface.Models;
using NatoliOrderInterface.Models.NAT01;
using NatoliOrderInterface.Models.NEC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        public NAT01Context Nat01context { get => nat01context; set => nat01context = value; }

        NAT01Context nat01context;

        public WorkOrder() { }

        /// <summary>
        /// Instance of a work order, complete with all details about the work order
        /// </summary>
        /// <param name="orderNumber"></param>
        /// <param name="_nat01context"></param>
        public WorkOrder(int orderNumber, NAT01Context _nat01context)
        {
            OrderNumber = orderNumber;
            nat01context = _nat01context;
            Finished = false;
            CanRunOnAutocell = false;

            // nat01context.OrderDetails.Where(o => o.OrderNo == OrderNumber).Load();

            using (var context = new NAT02Context())
            {
                Finished = context.EoiOrdersMarkedForChecking.Any(o => o.OrderNo == OrderNumber);
                context.Dispose();
            }

            LineItemCount = nat01context.OrderDetails.Where(o => o.OrderNo == OrderNumber * 100).Count();
            lineItems = nat01context.OrderDetails.Where(o => o.OrderNo == OrderNumber * 100).ToDictionary(kvp => (int)kvp.LineNumber, kvp => kvp.DetailTypeId.Trim());
            OrderHeader orderHeader = nat01context.OrderHeader.Where(o => o.OrderNo == OrderNumber * 100).FirstOrDefault();
            List<OrderDetails> orderDetails = nat01context.OrderDetails.Where(o => o.OrderNo == OrderNumber * 100).ToList();
            string repId = "";
            string csr = "*NO CSR*";
            if (nat01context.QuoteHeader.Where(q => q.QuoteNo == orderHeader.QuoteNumber && q.QuoteRevNo == orderHeader.QuoteRevNo).Any())
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

            SetInfo(orderHeader, csr, customerName, endUserName);
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
            productName = string.IsNullOrEmpty(orderHeader.ProductName) ? "" : orderHeader.ProductName.Trim();
            rushYOrN = string.IsNullOrEmpty(orderHeader.RushYorN) ? "" : orderHeader.RushYorN.Trim();
            paidRushFee = string.IsNullOrEmpty(orderHeader.PaidRushFee) ? "" : orderHeader.PaidRushFee.Trim();
        }
    }
}
