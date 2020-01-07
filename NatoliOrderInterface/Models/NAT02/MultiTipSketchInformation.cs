using System;
using System.Collections.Generic;
using System.Text;

namespace NatoliOrderInterface.Models
{
    public partial class MultiTipSketchInformation
    {
        public int ID { get; set; }
        public long DieNumber { get; set; }
        public decimal? Width { get; set; }
        public decimal? Length { get; set; }
        public int TotalNumberOfTips { get; set; }
        public string ToolType { get; set; }
        public decimal? DieOD { get; set; }
        public char AssembledOrSolid { get; set; }
        public int? OuterTipQty { get; set; }
        public decimal? OuterBoreCircle { get; set; }
        public decimal? OuterBoreAngle { get; set; }
        public int? MiddleTipQty { get; set; }
        public decimal? MiddleBoreCircle { get; set; }
        public decimal? MiddleBoreAngle { get; set; }
        public int? InnerTipQty { get; set; }
        public decimal? InnerBoreCircle { get; set; }
        public decimal? InnerBoreAngle { get; set; }
        public bool? CenterTip { get; set; }
        public decimal? ReferenceOD { get; set; }
        public decimal? ShoulderOffset { get; set; }
        public bool? CutDownShoulder { get; set; }
        public decimal? CutDownShoulderRadius { get; set; }
        public bool? LinearPattern { get; set; }
        public decimal? LinearSpacing { get; set; }
        public int? LinearRows { get; set; }
        public decimal? LinearSpacingColumns { get; set; }
        public int? LinearColumns { get; set; }
        public bool? Staggered { get; set; }
        public decimal? StaggerOffset { get; set; }

    }
}
