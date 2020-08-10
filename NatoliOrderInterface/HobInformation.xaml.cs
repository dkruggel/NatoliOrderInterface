using NatoliOrderInterface.Models;
using NatoliOrderInterface.Models.NAT01;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NatoliOrderInterface
{
    /// <summary>
    /// Interaction logic for HobInformation.xaml
    /// </summary>
    public partial class HobInformation : Window
    {
        Timer typingTimer = new Timer(800);
        string openedHobNumber = "";
        List<string> ShapeIDs { get; set; }
        public HobInformation()
        {
            InitializeComponent();
            typingTimer.Elapsed += TypingTimer_Elapsed;
            var _ = new NAT01Context();
            ShapeID.ItemsSource = _.ShapeFields.OrderBy(s => s.ShapeID).Select(s => s.ShapeID.ToString() + " - " + s.ShapeDescription.Trim()).ToList();
            HobStatus.ItemsSource = new List<string> { "D", "N", "Y", "R", "M" };
            CupCode.ItemsSource = _.CupConfig.OrderBy(c => c.CupID).Select(c => c.CupID.ToString() + " - " + c.Description.Trim()).ToList();
            BisectCode.ItemsSource = _.BisectCodes.OrderBy(b => b.ID).Select(b => b.ID.Trim() + " - " + b.Description.Trim()).ToList();
            HobClass.ItemsSource = _.ProductClass.OrderBy(p => p.ProductId).Select(p => p.ProductId.Trim() + " - " + p.Description.Trim()).ToList();
            DrawingYN.ItemsSource = new List<string> { "Y", "N" };
            DrawingType.ItemsSource = _.DrawingType.OrderBy(d => d.DrawingTypeId).Select(d => d.DrawingTypeId.Trim() + " - " + d.Description.Trim()).ToList();
            _.Dispose();
        }
        private void DieNumber_TextChanged(object sender, TextChangedEventArgs e)
        {
            typingTimer.Stop();
            typingTimer.Start();
        }
        private void HobNumber_TextChanged(object sender, TextChangedEventArgs e)
        {
            typingTimer.Stop();
            typingTimer.Start();
        }
        private void TypingTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                //if (!String.IsNullOrWhiteSpace(editedText))
                //{
                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() =>
                    {
                        if (DieNumber.IsFocused)
                        {
                            typingTimer.Stop();
                            bool dieInfoFilled = FillDieInfo();
                            if (dieInfoFilled)
                                AddDieButton.Content = "Update Die";
                            else
                                AddDieButton.Content = "Add Die";
                        }
                        else if (HobNumber.IsFocused)
                        {
                            typingTimer.Stop();
                            bool hobInfoFilled = FillHobInfo();
                            if (hobInfoFilled && openedHobNumber != HobNumber.Text)
                            {
                                AddHobButton.Content = "Update Hob";
                                TabletModelInformation tmi = new TabletModelInformation(HobNumber.Text);
                                openedHobNumber = HobNumber.Text;
                                DieNumber.Text = tmi.DieNumber;
                                bool dieInfoFilled = FillDieInfo();
                                CheckMatch(tmi.Width.ToString("#.0000"), Width.Text, Width);
                                CheckMatch((tmi.Width * 25.4).ToString("#.000"), WidthMetric.Text, WidthMetric);
                                CheckMatch(tmi.Length.ToString("#.0000"), Length.Text, Length);
                                CheckMatch((tmi.Length * 25.4).ToString("#.000"), LengthMetric.Text, LengthMetric);
                                CheckMatch(tmi.EndRadius.ToString("#.0000"), EndRadius.Text, EndRadius);
                                CheckMatch((tmi.EndRadius * 25.4).ToString("#.000"), EndRadiusMetric.Text, EndRadiusMetric);
                                CheckMatch(tmi.SideRadius.ToString("#.0000"), SideRadius.Text, SideRadius);
                                CheckMatch((tmi.SideRadius * 25.4).ToString("#.000"), SideRadiusMetric.Text, SideRadiusMetric);
                                CheckMatch(tmi.CupDepth.ToString("#.0000"), CupDepth.Text, CupDepth);
                                CheckMatch((tmi.CupDepth * 25.4).ToString("#.000"), CupDepthMetric.Text, CupDepthMetric);
                                CheckMatch(tmi.Land.ToString("#.0000"), Land.Text, Land);
                                CheckMatch((tmi.Land * 25.4).ToString("#.000"), LandMetric.Text, LandMetric);
                            }
                        }
                    }));
                //}
            }
            catch (Exception ex)
            {

            }
            typingTimer.Stop();
        }
        private void LoadData_Click(object sender, RoutedEventArgs e)
        {
            TabletModelInformation tmi = HobNumber.Text.Length > 0 ? new TabletModelInformation(HobNumber.Text) : new TabletModelInformation();
            DieNumber.Text = tmi.DieNumber;
            HobNumber.Text = tmi.HobNumber;

            // Check if hob or die exists
            var _ = new NAT01Context();
            bool hobExists = _.HobList.Any(h => h.HobNo == tmi.HobNumber && h.TipQty == short.Parse(TipQuantity.Text) && h.BoreCircle == float.Parse(BoreCircle.Text));
            bool dieExists = _.DieList.Any(d => d.DieId == tmi.DieNumber);
            _.Dispose();

            // Change Add Hob button to Update Hob if hob exists
            AddHobButton.Content = hobExists ? "Update Hob" : "Add Hob";

            // Change Add Die button to Update Die if die exists
            AddDieButton.Content = dieExists ? "Update Die" : "Add Die";

            TipQuantity.Text = "1";
            bool dieInfoFilled = FillDieInfo();
            
            // If die does not exist, pull in info from model
            if (!dieInfoFilled)
            {
                // Load die info from model
                Width.Text = tmi.Width.ToString("#.0000");
                WidthMetric.Text = (tmi.Width * 25.4).ToString("#.00");
                Length.Text = tmi.Length.ToString("#.0000");
                LengthMetric.Text = (tmi.Length * 25.4).ToString("#.00");
                EndRadius.Text = tmi.EndRadius.ToString("#.0000");
                EndRadiusMetric.Text = (tmi.EndRadius * 25.4).ToString("#.00");
                SideRadius.Text = tmi.SideRadius.ToString("#.0000");
                SideRadiusMetric.Text = (tmi.SideRadius * 25.4).ToString("#.00");
            }

            BoreCircle.Text = "0.0000";
            HobStatus.Text = "D";
            Size.Text = ""; // TODO: Build size string
            Shape.Text = ""; // TODO: Build shape string
            CupDepth.Text = tmi.CupDepth.ToString("#.0000");
            CupDepthMetric.Text = (tmi.CupDepth * 25.4).ToString("#.00");
            CupRadius.Text = tmi.CupRadius.ToString("#.0000");
            CupRadiusMetric.Text = (tmi.CupRadius * 25.4).ToString("#.00");
            Land.Text = tmi.Land.ToString("#.0000");
            LandMetric.Text = (tmi.Land * 25.4).ToString("#.00");
            LandRange.Text = "0.0000";
            LandRangeMetric.Text = "0.00";
            // Measurable Cup Depth
            // Cup Code
            // Bisect Code
            Owner.Text = ""; // TODO: Get owner customer number
            DateDesigned.Text = DateTime.Now.ToShortDateString();
            // Hob Class
            DrawingYN.Text = "Y";
            DrawingType.Text = "SW";
            // Program/Project Number
            NNumber.Text = "0";
        }
        private bool FillDieInfo()
        {
            var _ = new NAT01Context();
            try
            {
                DieList die = _.DieList.FirstOrDefault(d => d.DieId == DieNumber.Text.PadLeft(6));

                if (!(die is null))
                {
                    Width.Text = String.Format(die.WidthMinorAxis.ToString(), "#.0000");
                    WidthMetric.Text = String.Format(die.WidthMinorAxisM.ToString(), "#.000");
                    Length.Text = String.Format(die.LengthMajorAxis.ToString(), "#.0000");
                    LengthMetric.Text = String.Format(die.LengthMajorAxisM.ToString(), "#.000");
                    EndRadius.Text = String.Format(die.EndRadius.ToString(), "#.0000");
                    EndRadiusMetric.Text = String.Format(die.EndRadiusM.ToString(), "#.000");
                    SideRadius.Text = String.Format(die.SideRadius.ToString(), "#.0000");
                    SideRadiusMetric.Text = String.Format(die.SideRadiusM.ToString(), "#.000");
                    CornerRadius.Text = String.Format(die.CornerRadius.ToString(), "#.0000");
                    CornerRadiusMetric.Text = String.Format(die.CornerRadiusM.ToString(), "#.000");
                    BlendRadius.Text = String.Format(die.BlendingRadius.ToString(), "#.0000");
                    BlendRadiusMetric.Text = String.Format(die.BlendingRadiusM.ToString(), "#.000");
                    OD.Text = String.Format(die.OutsideDiameter.ToString(), "#.0000");
                    ODMetric.Text = String.Format(die.OutsideDiameterM.ToString(), "#.000");
                    ReferenceOD.Text = String.Format(die.RefOutsideDiameter.ToString(), "#.0000");
                    ReferenceODMetric.Text = String.Format(die.RefOutsideDiameterM.ToString(), "#.000");
                    ShapeID.Text = die.ShapeId.ToString();
                    // ShapeText.Text = _.ShapeFields.FirstOrDefault(sf => sf.ShapeID == die.ShapeId).ShapeDescription.Trim();
                    PlugGauge.Text = die.PlugGaugeStatus;
                    MasterDie.Text = die.MasterDieStatus;
                }
                else
                {
                    foreach (Grid grid in DieDockPanel.Children)
                    {
                        foreach (TextBox tb in grid.Children.OfType<TextBox>())
                        {
                            if (!(tb.Tag is null) && tb.Tag.ToString() == "Die")
                            {
                                tb.Text = "";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            finally
            {
                _.Dispose();
            }
            return true;
        }
        private bool FillHobInfo()
        {
            var _ = new NAT01Context();
            try
            {
                TipQuantity.Text = TipQuantity.Text.Trim().Length == 0 ? "1" : TipQuantity.Text.Trim();
                HobList hob = _.HobList.FirstOrDefault(h => h.HobNo == HobNumber.Text.PadLeft(6, '0') && h.TipQty == short.Parse(TipQuantity.Text));

                if (!(hob is null))
                {
                    BoreCircle.Text = String.Format(hob.BoreCircle.ToString(), "#.0000");
                    HobStatus.Text = hob.HobYorNorD;
                    Size.Text = hob.Size;
                    Shape.Text = hob.Shape;
                    CupDepth.Text = String.Format(hob.CupDepth.ToString(), "#.0000");
                    CupDepthMetric.Text = String.Format(hob.CupDepthM.ToString(), "#.000");
                    CupRadius.Text = String.Format(hob.CupRadius.ToString(), "#.0000");
                    CupRadiusMetric.Text = String.Format(hob.CupRadiusM.ToString(), "#.000");
                    Land.Text = String.Format(hob.Land.ToString(), "#.0000");
                    LandMetric.Text = String.Format(hob.LandM.ToString(), "#.000");
                    LandRange.Text = String.Format(hob.LandRange.ToString(), "#.0000");
                    LandRangeMetric.Text = String.Format(hob.LandRangeM.ToString(), "#.000");
                    MeasurableCupDepth.Text = String.Format(hob.MeasurableCd.ToString(), "#.0000");
                    MeasurableCupDepthMetric.Text = String.Format(hob.MeasurableCdm.ToString(), "#.000");
                    CupCode.Text = hob.CupCode.ToString();
                    BisectCode.Text = hob.BisectCode;
                    Owner.Text = hob.OwnerReservedFor;
                    DateDesigned.Text = hob.DateDesigned.Value.ToShortDateString();
                    HobClass.Text = hob.Class;
                    DrawingYN.Text = hob.DrawingYorN;
                    DrawingType.Text = hob.DrawingType;
                    ProgramNumber.Text = hob.ProgramNo.ToString();
                    NNumber.Text = hob.Nnumber.ToString();
                }
                else
                {
                    foreach (Grid grid in HobWrapPanel.Children)
                    {
                        foreach (TextBox tb in grid.Children.OfType<TextBox>())
                        {
                            if (!(tb.Tag is null) && tb.Tag.ToString() == "Hob")
                            {
                                tb.Text = "";
                            }
                        }
                    }
                    openedHobNumber = "";
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            finally
            {
                _.Dispose();
            }
            return true;
        }
        private void CheckMatch(string modelDim, string dbDim, TextBox tb)
        {
            modelDim = modelDim[0] == '.' ? "0" + modelDim : String.Format(modelDim, "#.0000");
            dbDim = dbDim.Length == 1 && dbDim[0] == '0' ? "0.0000" : !tb.Name.Contains("Metric") ? String.Format(dbDim, "#.0000").PadRight(6, '0') : String.Format(dbDim, "#.000").PadRight(5, '0');
            tb.BorderBrush = modelDim != dbDim ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.DarkGray);
            tb.ToolTip = new ToolTip()
            {
                Content = modelDim
            };
        }
        private void AddHob_Click(object sender, RoutedEventArgs e)
        {
            // Open connection with NAT01
            var _ = new NAT01Context();

            try
            {
                // Check if this is an update or insert
                bool hobExists = _.HobList.Any(h => h.HobNo == HobNumber.Text && h.TipQty == short.Parse(TipQuantity.Text) && h.BoreCircle == float.Parse(BoreCircle.Text));

                if (hobExists) // Update, get HobList object
                {
                    // Get object
                    HobList existingHob = _.HobList.First(h => h.HobNo == HobNumber.Text && h.TipQty == short.Parse(TipQuantity.Text) && h.BoreCircle == float.Parse(BoreCircle.Text));

                    // Set new info
                    existingHob.Shape = Shape.Text.ToUpper();
                    existingHob.DieId = DieNumber.Text;
                    existingHob.CupDepth = float.Parse(CupDepth.Text);
                    existingHob.Land = float.Parse(Land.Text);
                    existingHob.BisectCode = BisectCode.SelectedItem.ToString()[0..3].Trim();
                    existingHob.Class = HobClass.SelectedItem.ToString();
                    existingHob.DrawingYorN = DrawingYN.SelectedItem.ToString();
                    existingHob.HobYorNorD = HobStatus.SelectedItem.ToString();
                    existingHob.Note1 = Note1Hob.Text;
                    existingHob.Note2 = Note2Hob.Text;
                    existingHob.OwnerReservedFor = Owner.Text;
                    existingHob.DateDesigned = DateTime.Parse(DateDesigned.Text);
                    existingHob.CupCode = short.Parse(CupCode.SelectedItem.ToString()[0..2].Trim());
                    existingHob.Size = Size.Text;
                    existingHob.Note3 = Note3Hob.Text;
                    existingHob.DrawingType = DrawingType.SelectedItem.ToString()[0..2].Trim();
                    existingHob.LandRange = float.Parse(LandRange.Text);
                    existingHob.MeasurableCd = float.Parse(MeasurableCupDepth.Text);
                    existingHob.TipQty = short.Parse(TipQuantity.Text);
                    existingHob.ProgramNo = int.Parse(ProgramNumber.Text);
                    existingHob.BoreCircle = float.Parse(BoreCircle.Text);
                    existingHob.CupRadius = decimal.Parse(CupRadius.Text);
                    existingHob.CupRadiusM = decimal.Parse(CupRadiusMetric.Text);
                    existingHob.CupDepthM = decimal.Parse(CupDepthMetric.Text);
                    existingHob.LandM = decimal.Parse(LandMetric.Text);
                    existingHob.LandRangeM = decimal.Parse(LandRangeMetric.Text);
                    existingHob.MeasurableCdm = decimal.Parse(MeasurableCupDepthMetric.Text);
                    existingHob.Nnumber = int.Parse(NNumber.Text);

                    // Update table
                    _.HobList.Update(existingHob);
                }
                else // Insert, create HobList object
                {
                    // Create new object
                    HobList newHob = new HobList()
                    {
                        HobNo = HobNumber.Text,
                        Shape = Shape.Text.ToUpper(),
                        DieId = DieNumber.Text,
                        CupDepth = float.Parse(CupDepth.Text),
                        Land = float.Parse(Land.Text),
                        BisectCode = BisectCode.SelectedItem.ToString()[0..3].Trim(),
                        Class = HobClass.SelectedItem.ToString(),
                        DrawingYorN = DrawingYN.SelectedItem.ToString(),
                        HobYorNorD = HobStatus.SelectedItem.ToString(),
                        Note1 = Note1Hob.Text,
                        Note2 = Note2Hob.Text,
                        OwnerReservedFor = Owner.Text,
                        DateDesigned = DateTime.Parse(DateDesigned.Text),
                        CupCode = short.Parse(CupCode.SelectedItem.ToString()[0..2].Trim()),
                        Size = Size.Text,
                        Note3 = Note3Hob.Text,
                        DrawingType = DrawingType.SelectedItem.ToString()[0..2].Trim(),
                        LandRange = float.Parse(LandRange.Text),
                        MeasurableCd = float.Parse(MeasurableCupDepth.Text),
                        TipQty = short.Parse(TipQuantity.Text),
                        ProgramNo = int.Parse(ProgramNumber.Text),
                        BoreCircle = float.Parse(BoreCircle.Text),
                        Flush = true,
                        CupRadius = decimal.Parse(CupRadius.Text),
                        CupRadiusM = decimal.Parse(CupRadiusMetric.Text),
                        CupDepthM = decimal.Parse(CupDepthMetric.Text),
                        LandM = decimal.Parse(LandMetric.Text),
                        LandRangeM = decimal.Parse(LandRangeMetric.Text),
                        MeasurableCdm = decimal.Parse(MeasurableCupDepthMetric.Text),
                        Nnumber = int.Parse(NNumber.Text),
                        Dimple = false
                    };

                    // Insert into table
                    _.HobList.Add(newHob);
                }

                // Save changes to database
                _.SaveChanges();
            }
            catch (Exception ex)
            {
                //TODO: Log error
            }
            finally
            {
                // Dispose of connection
                _.Dispose();
            }
        }
        private void AddDie_Click(object sender, RoutedEventArgs e)
        {
            // Open connection with NAT01
            var _ = new NAT01Context();

            try
            {
                // Check if this is an update or insert
                bool dieExists = _.DieList.Any(d => d.DieId == DieNumber.Text);

                if (dieExists) // Update, get DieList object
                {
                    // Get object
                    DieList existingDie = _.DieList.First(d => d.DieId == DieNumber.Text);

                    // Set new info
                    existingDie.LengthMajorAxis = float.Parse(Length.Text);
                    existingDie.WidthMinorAxis = float.Parse(Width.Text);
                    existingDie.EndRadius = float.Parse(EndRadius.Text);
                    existingDie.CornerRadius = float.Parse(CornerRadius.Text);
                    existingDie.OutsideDiameter = float.Parse(OD.Text);
                    existingDie.SideRadius = float.Parse(SideRadius.Text);
                    existingDie.Note1 = Note1Die.Text;
                    existingDie.Note2 = Note2Die.Text;
                    existingDie.Note3 = Note3Die.Text;
                    existingDie.BlendingRadius = float.Parse(BlendRadius.Text);
                    existingDie.RefOutsideDiameter = float.Parse(ReferenceOD.Text);
                    existingDie.ShapeId = short.Parse(ShapeID.Text);
                    existingDie.LengthMajorAxisM = decimal.Parse(LengthMetric.Text);
                    existingDie.WidthMinorAxisM = decimal.Parse(WidthMetric.Text);
                    existingDie.EndRadiusM = decimal.Parse(EndRadiusMetric.Text);
                    existingDie.CornerRadiusM = decimal.Parse(CornerRadiusMetric.Text);
                    existingDie.SideRadiusM = decimal.Parse(SideRadiusMetric.Text);
                    existingDie.BlendingRadiusM = decimal.Parse(BlendRadiusMetric.Text);
                    existingDie.OutsideDiameterM = decimal.Parse(ODMetric.Text);
                    existingDie.RefOutsideDiameterM = decimal.Parse(ReferenceODMetric.Text);

                    // Update table
                    _.DieList.Update(existingDie);
                }
                else // Insert, create DieList object
                {
                    // Create new object
                    DieList newDie = new DieList()
                    {
                        LengthMajorAxis = float.Parse(Length.Text),
                        WidthMinorAxis = float.Parse(Width.Text),
                        EndRadius = float.Parse(EndRadius.Text),
                        CornerRadius = float.Parse(CornerRadius.Text),
                        OutsideDiameter = float.Parse(OD.Text),
                        SideRadius = float.Parse(SideRadius.Text),
                        Note1 = Note1Die.Text,
                        Note2 = Note2Die.Text,
                        Note3 = Note3Die.Text,
                        BlendingRadius = float.Parse(BlendRadius.Text),
                        RefOutsideDiameter = float.Parse(ReferenceOD.Text),
                        ShapeId = short.Parse(ShapeID.Text),
                        LengthMajorAxisM = decimal.Parse(LengthMetric.Text),
                        WidthMinorAxisM = decimal.Parse(WidthMetric.Text),
                        EndRadiusM = decimal.Parse(EndRadiusMetric.Text),
                        CornerRadiusM = decimal.Parse(CornerRadiusMetric.Text),
                        SideRadiusM = decimal.Parse(SideRadiusMetric.Text),
                        BlendingRadiusM = decimal.Parse(BlendRadiusMetric.Text),
                        OutsideDiameterM = decimal.Parse(ODMetric.Text),
                        RefOutsideDiameterM = decimal.Parse(ReferenceODMetric.Text)
                    };

                    // Insert into table
                    _.DieList.Add(newDie);
                }

                // Save changes to database
                _.SaveChanges();
            }
            catch (Exception ex)
            {
                //TODO: Log error
            }
            finally
            {
                // Dispose of connection
                _.Dispose();
            }
        }
    }
}

// NEED TO COMPLETE
// Grab info from project
// Save info in a staged area until verified