using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models
{
    public partial class TravellerScansAudit
    {
        public int TsaId { get; set; }
        public string TransactionType { get; set; }
        public DateTime PairingId { get; set; }
        public string WorkOrderToDisplay { get; set; }
        public string TravellerNo { get; set; }
        public short DeletedFlag { get; set; }
        public short AdjustedFlag { get; set; }
        public string WorkOrderNumber { get; set; }
        public int OrderNumber { get; set; }
        public short OrderLineNumber { get; set; }
        public string OrderDetailTypeId { get; set; }
        public DateTime ScanTimeStamp { get; set; }
        public DateTime ScanOn { get; set; }
        public DateTime? ScanOnDate { get; set; }
        public DateTime? ScanOff { get; set; }
        public DateTime? ScanOffDate { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string DepartmentCode { get; set; }
        public string DepartmentDesc { get; set; }
        public string MachineCode { get; set; }
        public string MachineDesc { get; set; }
        public string OperationCode { get; set; }
        public string OperationDesc { get; set; }
        public short QuantityOrdered { get; set; }
        public short QuantityToMfg { get; set; }
        public short? OnQuantityGood { get; set; }
        public short? OnQuantityScrap { get; set; }
        public string OnScrapReasonCode { get; set; }
        public string OnScrapReasonDesc { get; set; }
        public DateTime? OnAdcsyncDateTime { get; set; }
        public short OnAdcsyncFlag { get; set; }
        public short? OffQuantityGood { get; set; }
        public short? OffQuantityScrap { get; set; }
        public string OffScrapReasonCode { get; set; }
        public string OffScrapReasonDesc { get; set; }
        public string TerminalId { get; set; }
        public string NtuserId { get; set; }
        public short OffAdcsyncFlag { get; set; }
        public DateTime? OffAdcsyncDateTime { get; set; }
        public DateTime? BatchId { get; set; }
        public int? OnNcmincidentNo { get; set; }
        public int? OffNcmincidentNo { get; set; }
        public string EmployeeType { get; set; }
        public string LeadEmployeeCode { get; set; }
        public string LeadEmployeeName { get; set; }
        public string SupervisorEmployeeCode { get; set; }
        public string SupervisorEmployeeName { get; set; }
        public string ManagerEmployeeCode { get; set; }
        public string ManagerEmployeeName { get; set; }
        public string OrderDetailTypeDescription { get; set; }
        public short? ReworkQty { get; set; }
        public string ReworkCode { get; set; }
        public string ReworkDesc { get; set; }
        public int? ReworkIncidentNo { get; set; }
        public string MoeCardCode { get; set; }
        public int? CurrentGoal { get; set; }
        public int? NextLevelGoal { get; set; }
        public DateTime? PayrollDate { get; set; }
        public byte? PayrollShiftNo { get; set; }
        public string PayrollShiftName { get; set; }
        public DateTime? FpiPairingId { get; set; }
        public DateTime? FpiTimestamp { get; set; }
        public bool? FpiPassed { get; set; }
    }
}
