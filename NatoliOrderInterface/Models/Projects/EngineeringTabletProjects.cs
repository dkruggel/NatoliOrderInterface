using System;
using System.Collections.Generic;
using System.Text;

namespace NatoliOrderInterface.Models.Projects
{
    public partial class EngineeringTabletProjects
    {
        public string ProjectNumber { get; set; }
        public string RevNumber { get; set; }
        public bool UpperRequired { get; set; }
        public bool LowerRequired { get; set; }
        public bool ShortRejectRequired { get; set; }
        public bool LongRejectRequired { get; set; }
        public bool? FilmCoated { get; set; }
        public string FilmCoatType { get; set; }
        public bool PrePick { get; set; }
        public decimal? PrePickAmount { get; set; }
        public string PrePickUnits { get; set; }
        public bool Taper { get; set; }
        public decimal? TaperAmount { get; set; }
        public string TaperUnits { get; set; }
        public decimal? Density { get; set; }
        public string DensityUnits { get; set; }
        public decimal? Mass { get; set; }
        public string MassUnits { get; set; }
        public decimal? Volume { get; set; }
        public string VolumeUnits { get; set; }
        public decimal? TargetThickness { get; set; }
        public string TargetThicknessUnits { get; set; }
    }
}
