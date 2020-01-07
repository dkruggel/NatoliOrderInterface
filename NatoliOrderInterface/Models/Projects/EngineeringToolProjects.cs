using System;
using System.Collections.Generic;
using System.Text;

namespace NatoliOrderInterface.Models.Projects
{
    public partial class EngineeringToolProjects
    {
        public string ProjectNumber { get; set; }
        public string RevNumber { get; set; }
        public bool Alignment { get; set; }
        public string AlignmentSteelID { get; set; }
        public bool Die { get; set; }
        public string DieSteelID { get; set; }
        public bool DieAssembly { get; set; }
        public bool DieComponent { get; set; }
        public string DieComponentSteelID { get; set; }
        public bool DieHolder { get; set; }
        public string DieHolderSteelID { get; set; }
        public bool DieInsert { get; set; }
        public string DieInsertSteelID { get; set; }
        public bool DiePlate { get; set; }
        public string DiePlateSteelID { get; set; }
        public bool DieSegment { get; set; }
        public string DieSegmentSteelID { get; set; }
        public bool Key { get; set; }
        public string KeySteelID { get; set; }
        public bool LowerPunch { get; set; }
        public string LowerPunchSteelID { get; set; }
        public bool LowerAssembly { get; set; }
        public bool LowerCap { get; set; }
        public string LowerCapSteelID { get; set; }
        public bool LowerCoreRod { get; set; }
        public string LowerCoreRodSteelID { get; set; }
        public bool LowerCoreRodKey { get; set; }
        public string LowerCoreRodKeySteelID { get; set; }
        public bool LowerCoreRodKeyCollar { get; set; }
        public string LowerCoreRodKeyCollarSteelID { get; set; }
        public bool LowerCoreRodPunch { get; set; }
        public string LowerCoreRodPunchSteelID { get; set; }
        public bool LowerHolder { get; set; }
        public string LowerHolderSteelID { get; set; }
        public bool LowerHead { get; set; }
        public string LowerHeadSteelID { get; set; }
        public bool LowerTip { get; set; }
        public string LowerTipSteelID { get; set; }
        public bool Misc { get; set; }
        public string MiscSteelID { get; set; }
        public bool ShortRejectPunch { get; set; }
        public string ShortRejectPunchSteelID { get; set; }
        public bool ShortRejectAssembly { get; set; }
        public bool ShortRejectCap { get; set; }
        public string ShortRejectCapSteelID { get; set; }
        public bool ShortRejectHolder { get; set; }
        public string ShortRejectHolderSteelID { get; set; }
        public bool ShortRejectHead { get; set; }
        public string ShortRejectHeadSteelID { get; set; }
        public bool ShortRejectTip { get; set; }
        public string ShortRejectTipSteelID { get; set; }
        public bool LongRejectPunch { get; set; }
        public string LongRejectPunchSteelID { get; set; }
        public bool LongRejectAssembly { get; set; }
        public bool LongRejectCap { get; set; }
        public string LongRejectCapSteelID { get; set; }
        public bool LongRejectHolder { get; set; }
        public string LongRejectHolderSteelID { get; set; }
        public bool LongRejectHead { get; set; }
        public string LongRejectHeadSteelID { get; set; }
        public bool LongRejectTip { get; set; }
        public string LongRejectTipSteelID { get; set; }
        public bool UpperPunch { get; set; }
        public string UpperPunchSteelID { get; set; }
        public bool UpperAssembly { get; set; }
        public bool UpperCap { get; set; }
        public string UpperCapSteelID { get; set; }
        public bool UpperHolder { get; set; }
        public string UpperHolderSteelID { get; set; }
        public bool UpperHead { get; set; }
        public string UpperHeadSteelID { get; set; }
        public bool UpperTip { get; set; }
        public string UpperTipSteelID { get; set; }
        public string MachineNotes { get; set; }
        public string KeyType { get; set; }
        public decimal? KeyAngle { get; set; }
        public bool? KeyIsClockWise { get; set; }
        public bool UpperKeyed { get; set; }
        public bool LowerKeyed { get; set; }
        public bool ShortRejectKeyed { get; set; }
        public bool LongRejectKeyed { get; set; }
        public string UpperGrooveType { get; set; }
        public string LowerGrooveType { get; set; }
        public bool UpperGroove { get; set; }
        public bool LowerGroove { get; set; }
        public bool ShortRejectGroove { get; set; }
        public bool LongRejectGroove { get; set; }
        public string HeadType { get; set; }
        public bool CarbideTips { get; set; }
    }
}
