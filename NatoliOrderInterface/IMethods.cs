using DK.WshRuntime;
using NatoliOrderInterface.Models;
using NatoliOrderInterface.Models.DriveWorks;
using NatoliOrderInterface.Models.NAT01;
using NatoliOrderInterface.Models.Projects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace NatoliOrderInterface
{
    
    public interface IMethods
    {
        
        /// <summary>
        /// Checks if string 'input' contains any strings 'containsKeywords' using Default StringComparison.InvariantCulture. Returns bool.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="containsKeywords"></param>
        /// <param name="comparisonType"></param>
        /// <returns></returns>
        public static bool ContainsAny(string input, IEnumerable<string> containsKeywords, StringComparison comparisonType = StringComparison.InvariantCulture)
        {
            return containsKeywords.Any(keyword => input.IndexOf(keyword, comparisonType) >= 0);
        }
        /// <summary>
        /// Returns all Shape Descriptions as a list of strings from NAT01.ShapeFields.
        /// </summary>
        /// <returns></returns>
        public static List<string> GetShapeDescriptionsItemsSource()
        {
            using var _nat01Context = new NAT01Context();
            List<ShapeFields> shapeFields = _nat01Context.ShapeFields.Where(p => !string.IsNullOrWhiteSpace(p.ShapeDescription)).ToList();
            List<string> descriptions = shapeFields.Select(p => p.ShapeDescription.Trim()).ToList();
            _nat01Context.Dispose();
            return descriptions;
        }
        /// <summary>
        /// Returns possible project due dates in a format of "x Day(s) | mm/dd/yyyy" as a list of strings.
        /// </summary>
        /// <returns></returns>
        public static List<string> GetDueDatesItemsSource()
        {
            List<string> openDates = new List<string>();
            for (short i = 0; i < 15; i++)
            {
                DateTime day = DateTime.Today.AddDays(i);
                openDates.Add(i + " Day(s) | " + day.ToString("d"));
            }
            return openDates;
        }
        /// <summary>
        /// Returns "{CupCodes} - {Description}"as a list of strings from NAT01.CupConfig.
        /// </summary>
        /// <returns></returns>
        public static List<string> GetCupTypeItemsSource()
        {
            using var _nat01Context = new NAT01Context();
            List<string> cupTypes = new List<string>() { { "" } };
            cupTypes.AddRange(_nat01Context.CupConfig.Where(c => c.CupID > 0).Select(c => c.CupID + " - " + c.Description.Trim()).ToList());
            _nat01Context.Dispose();
            return cupTypes;
        }
        /// <summary>
        /// Returns all Steel ID's as a list of strings from NAT01.SteelType.
        /// </summary>
        /// <returns></returns>
        public static List<string> GetSteelIDItemsSource()
        {
            using (var _nat01Context = new NAT01Context())
            {
                List<string> steelIDs = _nat01Context.SteelType.OrderBy(s => s.TypeId).Select(s => s.TypeId.Trim()).ToList();
                return steelIDs;
            }
        }
        /// <summary>
        /// Returns all Users as a list of strings from Driveworks.UserCustomizations (group info).
        /// </summary>
        /// <returns></returns>
        public static List<string> GetDWCSRs()
        {
            using var _driveworks_USRContext = new Driveworks_USRContext();
            List<UserCustomizations> userCustomizations = _driveworks_USRContext.UserCustomizations.OrderBy(uc => uc.User).ToList();
            List<string> dwCSRUsers = new List<string> { "" };
            foreach (UserCustomizations userCustomization in userCustomizations)
            {
                dwCSRUsers.Add(userCustomization.User.ToString().Trim());
            }
            _driveworks_USRContext.Dispose();
            dwCSRUsers.Remove("Admin");
            return dwCSRUsers;

        }
        /// <summary>
        /// Returns a "blank" EngineeringProjects that can be used to fill in the window before creating a project.
        /// It includes user preferences.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="projectNumber"></param>
        /// <param name="projectRevNumber"></param>
        /// <returns></returns>
        public static EngineeringProjects GetBlankEngineeringProject(User user, string projectNumber, string projectRevNumber)
        {
            using var _driveworks_USRContext = new Driveworks_USRContext();
            EngineeringProjects engineeringProject = new EngineeringProjects
            {
                ProjectNumber = projectNumber,
                RevNumber = projectRevNumber,
                ActiveProject = false,
                QuoteNumber = "",
                QuoteRevNumber = "",
                RefProjectNumber = "",
                RefProjectRevNumber = "",
                RefQuoteNumber = "",
                RefQuoteRevNumber = "",
                RefOrderNumber = "",
                CSR = user == null ? new User().GetDWPrincipalId() : user.GetDWPrincipalId(),
                ReturnToCSR = "",
                CustomerNumber = "",
                CustomerName = "",
                ShipToNumber = "",
                ShipToLocNumber = "",
                ShipToName = "",
                EndUserNumber = "",
                EndUserLocNumber = "",
                EndUserName = "",
                UnitOfMeasure = _driveworks_USRContext.UserCustomizations.Any(u => u.User == user.GetDWPrincipalId()) ? _driveworks_USRContext.UserCustomizations.First(u => u.User == user.GetDWPrincipalId()).UnitOfMeasure.Split("|").First() : "IN",
                Product = "",
                Attention = "",
                MachineNumber = "",
                DieNumber = "",
                DieShape = "",
                Width = null,
                Length = null,
                UpperHobNumber = "",
                UpperHobDescription = "",
                UpperCupType = null,
                UpperCupDepth = null,
                UpperLand = null,
                LowerHobNumber = "",
                LowerHobDescription = "",
                LowerCupType = null,
                LowerCupDepth = null,
                LowerLand = null,
                ShortRejectHobNumber = "",
                ShortRejectHobDescription = "",
                ShortRejectCupType = null,
                ShortRejectCupDepth = null,
                ShortRejectLand = null,
                LongRejectHobNumber = "",
                LongRejectHobDescription = "",
                LongRejectCupType = null,
                LongRejectCupDepth = null,
                LongRejectLand = null,
                UpperTolerances = "",
                LowerTolerances = "",
                ShortRejectTolerances = "",
                LongRejectTolerances = "",
                DieTolerances = "",
                Notes = "",
                TimeSubmitted = DateTime.UtcNow,
                DueDate = DateTime.MaxValue,
                Priority = false,
                TabletStarted = false,
                TabletStartedDateTime = null,
                TabletStartedBy = "",
                TabletDrawn = false,
                TabletDrawnDateTime = null,
                TabletDrawnBy = "",
                TabletSubmitted = false,
                TabletSubmittedDateTime = null,
                TabletSubmittedBy = "",
                TabletChecked = false,
                TabletCheckedDateTime = null,
                TabletCheckedBy = "",
                ToolStarted = false,
                ToolStartedDateTime = null,
                ToolStartedBy = "",
                ToolDrawn = false,
                ToolDrawnDateTime = null,
                ToolDrawnBy = "",
                ToolSubmitted = false,
                ToolSubmittedDateTime = null,
                ToolSubmittedBy = "",
                ToolChecked = false,
                ToolCheckedDateTime = null,
                ToolCheckedBy = "",
                NewDrawing = false,
                UpdateExistingDrawing = false,
                UpdateTextOnDrawing = false,
                PerSampleTablet = false,
                RefTabletDrawing = false,
                PerSampleTool = false,
                RefToolDrawing = false,
                PerSuppliedPicture = false,
                RefNatoliDrawing = false,
                RefNonNatoliDrawing = false,
                MultiTipSketch = false,
                MultiTipSketchID = null,
                NumberOfTips = 1,
                BinLocation = "",
                MultiTipSolid = false,
                MultiTipAssembled = false,
                OnHold = false,
                OnHoldComment = "",
                OnHoldDateTime = null,
                RevisedBy = null,
                Changes = null,
            };
            _driveworks_USRContext.Dispose();
            return engineeringProject;
        }
        /// <summary>
        /// Tries to create directory link (shortcut) from directory1 to directory2 and vice-versa.
        /// If directory2 does not exist, tries directory 3
        /// </summary>
        /// <param name="directory1"></param>
        /// <param name="directory2"></param>
        /// <param name="directory3"></param>
        /// <returns>MessageBoxOverloads</returns>
        public static (string Message, string Caption, MessageBoxButton Button, MessageBoxImage Image, MessageBoxResult Result) LinkFolders(string directory1, string directory2, string directory3 = "")
        {
            try
            {
                if (string.IsNullOrEmpty(directory2) || !System.IO.Directory.Exists(directory2) && !string.IsNullOrEmpty(directory3))
                {
                    directory2 = directory3;
                }
                if (!string.IsNullOrEmpty(directory1) && !string.IsNullOrEmpty(directory2) && System.IO.Directory.Exists(directory1) && System.IO.Directory.Exists(directory2))
                {
                    directory1 = directory1.Last() == '\\' ? directory1.Remove(directory1.Length - 1) : directory1;
                    directory2 = directory2.Last() == '\\' ? directory2.Remove(directory2.Length - 1) : directory2;
                    string name1 = directory1.Remove(0, directory1.LastIndexOf('\\') + 1);
                    string name2 = directory2.Remove(0, directory2.LastIndexOf('\\') + 1);
                    string rootName1 = directory1.Remove(directory1.LastIndexOf('\\')).Remove(0, directory1.Remove(directory1.LastIndexOf('\\')).LastIndexOf('\\') + 1);
                    string rootName2 = directory2.Remove(directory2.LastIndexOf('\\')).Remove(0, directory2.Remove(directory2.LastIndexOf('\\')).LastIndexOf('\\') + 1);

                    List<string> directories = new List<string>() { directory1, directory2 };
                    foreach (string directory in directories)
                    {
                        string[] existingLinks = System.IO.Directory.GetFiles(directory).Where(f => f.Contains(".lnk")).ToArray();
                        if (existingLinks.Any())
                        {
                            string rootName = directory.Remove(directory.LastIndexOf('\\')).Remove(0, directory.Remove(directory.LastIndexOf('\\')).LastIndexOf('\\') + 1);
                            string name = directory.Remove(0, directory.LastIndexOf('\\') + 1);
                            string message = "In the folder, " + rootName + "\\" + name + ",\nThere are already link(s) to:\n";
                            foreach (string link in existingLinks)
                            {
                                message += " " + link.Remove(link.Length - 4).Remove(0, link.Remove(link.Length - 4).LastIndexOf('\\') + 1) + "\n";
                            }
                            message += "Would you like to delete the link(s)?";
                            MessageBoxResult result = MessageBox.Show(message, "Existing Links", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                            if (result == MessageBoxResult.Yes)
                            {
                                foreach (string link in existingLinks)
                                {
                                    System.IO.File.Delete(link);
                                }
                            }
                        }
                    }
                    if (!System.IO.File.Exists(directory1 + "\\" + name2 + ".lnk"))
                    {
                        WshInterop.CreateShortcut(
                            directory1 + "\\" + name2 + ".lnk", "",
                            directory2, "", "");
                    }
                    if (!System.IO.File.Exists(directory2 + "\\" + name1 + ".lnk"))
                    {
                        WshInterop.CreateShortcut(
                        directory2 + "\\" + name1 + ".lnk", "",
                        directory1, "", "");
                    }
                    return (Message: rootName1 + " folder " + name1 + " was successfully linked with " + rootName2 + " folder " + name2 + "!", Caption: "Success", Button: MessageBoxButton.OK, Image: MessageBoxImage.Information, Result: MessageBoxResult.OK);
                }
                else
                {
                    return (Message: "One of the folders,\n" + directory1 + "\n or \n" + directory2 + ",\n do not exists.", Caption: "Directory Does Not Exist", Button: MessageBoxButton.OK, Image: MessageBoxImage.Error, Result: MessageBoxResult.OK);
                }
            }

            catch
            {
                return (Message: "Something went wrong!" + "\n" + "Nothing was linked.", Caption: "Oops", Button: MessageBoxButton.OK, Image: MessageBoxImage.Error, Result: MessageBoxResult.OK);
            }

        }
        /// <summary>
        /// Returns true if the type is of SimpleType.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsSimpleType(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                // nullable type, check if the nested type is simple.
                return IsSimpleType(type.GetGenericArguments()[0]);
            }
            return type.IsPrimitive
              || type.IsEnum
              || type.Equals(typeof(string))
              || type.Equals(typeof(decimal));
        }
        /// <summary>
        /// Compares two Classes Properties for differences and returns a list of strings describing what changed.
        /// A => original class values
        /// B => new class values
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static List<string> GetChangedProperties<T>(T A, T B)
        {
            if (A != null && B != null)
            {
                var type = typeof(T);
                var allProperties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                var allSimpleProperties = allProperties.Where(pi => IsSimpleType(pi.PropertyType));
                var unequalProperties =
                       from pi in allSimpleProperties
                       let AValue = type.GetProperty(pi.Name).GetValue(A, null)
                       let BValue = type.GetProperty(pi.Name).GetValue(B, null)
                       where AValue != BValue && (AValue == null || !AValue.Equals(BValue))
                       select pi.Name + ": " + (string.IsNullOrEmpty((AValue ?? "null").ToString()) ? "null" : (AValue ?? "null").ToString()) + " => " + (string.IsNullOrEmpty((BValue ?? "null").ToString()) ? "null" : (BValue ?? "null").ToString());
                return unequalProperties.ToList();
            }
            else
            {
                throw new ArgumentNullException("You need to provide 2 non-null objects");
            }
        }
        /// <summary>
        /// Starts an EngineeringProject based on input projectType "TABLETS" or "TOOLS".
        /// </summary>
        /// <param name="projectNumber"></param>
        /// <param name="projectRevNumber"></param>
        /// <param name="projectType"></param>
        /// <param name="user"></param>
        public static void StartProject(string projectNumber, string projectRevNumber, string projectType, User user)
        {
            using var _projectsContext = new ProjectsContext();
            using var _driveworksContext = new DriveWorksContext();
            try
            {
                if (projectType == "TABLETS")
                {
                    EngineeringProjects engineeringProject = _projectsContext.EngineeringProjects.First(p => p.ProjectNumber == projectNumber && p.RevNumber == projectRevNumber);
                    engineeringProject.TabletStarted = true;
                    engineeringProject.TabletStartedDateTime = DateTime.UtcNow;
                    engineeringProject.TabletStartedBy = user.GetDWPrincipalId();

                    _projectsContext.SaveChanges();
                }
                else if (projectType == "TOOLS")
                {
                    EngineeringProjects engineeringProject = _projectsContext.EngineeringProjects.First(p => p.ProjectNumber == projectNumber && p.RevNumber == projectRevNumber);
                    engineeringProject.ToolStarted = true;
                    engineeringProject.ToolStartedDateTime = DateTime.UtcNow;
                    engineeringProject.ToolStartedBy = user.GetDWPrincipalId();

                    _projectsContext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            _projectsContext.Dispose();
            _driveworksContext.Dispose();
        }
        /// <summary>
        /// Draws an EngineeringProject based on input projectType "TABLETS" or "TOOLS".
        /// </summary>
        /// <param name="projectNumber"></param>
        /// <param name="projectRevNumber"></param>
        /// <param name="projectType"></param>
        /// <param name="user"></param>
        public static void DrawProject(string projectNumber, string projectRevNumber, string projectType, User user)
        {
            using var _projectsContext = new ProjectsContext();
            using var _driveworksContext = new DriveWorksContext();
            try
            {
                if (projectType == "TABLETS")
                {
                    EngineeringProjects engineeringProject = _projectsContext.EngineeringProjects.First(p => p.ProjectNumber == projectNumber && p.RevNumber == projectRevNumber);
                    engineeringProject.TabletDrawn = true;
                    engineeringProject.TabletDrawnDateTime = DateTime.UtcNow;
                    engineeringProject.TabletDrawnBy = user.GetDWPrincipalId();

                    _projectsContext.SaveChanges();
                }
                else if (projectType == "TOOLS")
                {
                    EngineeringProjects engineeringProject = _projectsContext.EngineeringProjects.First(p => p.ProjectNumber == projectNumber && p.RevNumber == projectRevNumber);
                    engineeringProject.ToolDrawn = true;
                    engineeringProject.ToolDrawnDateTime = DateTime.UtcNow;
                    engineeringProject.ToolDrawnBy = user.GetDWPrincipalId();

                    _projectsContext.SaveChanges();

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            _projectsContext.Dispose();
            _driveworksContext.Dispose();
        }
        /// <summary>
        /// Submits an EngineeringProject based on input projectType "TABLETS" or "TOOLS".
        /// </summary>
        /// <param name="projectNumber"></param>
        /// <param name="projectRevNumber"></param>
        /// <param name="projectType"></param>
        /// <param name="user"></param>
        public static void SubmitProject(string projectNumber, string projectRevNumber, string projectType, User user)
        {
            using var _projectsContext = new ProjectsContext();
            using var _driveworksContext = new DriveWorksContext();
            try
            {
                if (projectType == "TABLETS")
                {
                    EngineeringProjects engineeringProject = _projectsContext.EngineeringProjects.First(p => p.ProjectNumber == projectNumber && p.RevNumber == projectRevNumber);
                    engineeringProject.TabletSubmitted = true;
                    engineeringProject.TabletSubmittedDateTime = DateTime.UtcNow;
                    engineeringProject.TabletSubmittedBy = user.GetDWPrincipalId();

                    _projectsContext.SaveChanges();
                }
                else if (projectType == "TOOLS")
                {
                    EngineeringProjects engineeringProject = _projectsContext.EngineeringProjects.First(p => p.ProjectNumber == projectNumber && p.RevNumber == projectRevNumber);
                    engineeringProject.ToolSubmitted = true;
                    engineeringProject.ToolSubmittedDateTime = DateTime.UtcNow;
                    engineeringProject.ToolSubmittedBy = user.GetDWPrincipalId();

                    _projectsContext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            _projectsContext.Dispose();
            _driveworksContext.Dispose();
        }
        /// <summary>
        /// Checks an EngineeringProject based on input projectType "TABLETS" or "TOOLS".
        /// </summary>
        /// <param name="projectNumber"></param>
        /// <param name="projectRevNumber"></param>
        /// <param name="projectType"></param>
        /// <param name="user"></param>
        public static void CheckProject(string projectNumber, string projectRevNumber, string projectType, User user)
        {
            using var _projectsContext = new ProjectsContext();
            using var _driveworksContext = new DriveWorksContext();
            using var _nat02Context = new NAT02Context();
            try
            {
                if (projectType == "TABLETS")
                {
                    bool _tools = _projectsContext.EngineeringToolProjects.Any();
                    EngineeringProjects engineeringProject = _projectsContext.EngineeringProjects.First(p => p.ProjectNumber == projectNumber && p.RevNumber == projectRevNumber);
                    engineeringProject.TabletChecked = true;
                    engineeringProject.TabletCheckedDateTime = DateTime.UtcNow;
                    engineeringProject.TabletCheckedBy = user.GetDWPrincipalId();
                    _projectsContext.SaveChanges();
                    // Removes from engineeringprojects, trigger puts into archive table, then must specify if it was checked or canceled
                    if (!_tools)
                    {
                        _projectsContext.Remove(engineeringProject);
                        _projectsContext.SaveChanges();
                        while (!_projectsContext.EngineeringArchivedProjects.Any(p => p.ProjectNumber == projectNumber && p.RevNumber == projectRevNumber))
                        {
                            System.Threading.Thread.Sleep(500);
                        }
                        EngineeringArchivedProjects engineeringArchivedProject = _projectsContext.EngineeringArchivedProjects.First(p => p.ProjectNumber == projectNumber && p.RevNumber == projectRevNumber);
                        engineeringArchivedProject.ArchivedFromCheck = true;
                        engineeringArchivedProject.ArchivedBy = user.GetDWPrincipalId();


                        List<string> _CSRs = new List<string>();

                        if (!string.IsNullOrEmpty(_projectsContext.EngineeringProjects.First(p => p.ProjectNumber == projectNumber && p.RevNumber == projectRevNumber).CSR))
                        {
                            _CSRs.Add(_projectsContext.EngineeringProjects.First(p => p.ProjectNumber == projectNumber && p.RevNumber == projectRevNumber).CSR);
                        }
                        if (!string.IsNullOrEmpty(_projectsContext.EngineeringProjects.First(p => p.ProjectNumber == projectNumber && p.RevNumber == projectRevNumber).ReturnToCSR))
                        {
                            _CSRs.Add(_projectsContext.EngineeringProjects.First(p => p.ProjectNumber == projectNumber && p.RevNumber == projectRevNumber).ReturnToCSR);
                        }


                        EoiProjectsFinished finished = new EoiProjectsFinished();
                        finished.ProjectNumber = int.Parse(projectNumber);
                        finished.RevisionNumber = int.Parse(projectRevNumber);
                        finished.Csr = _CSRs[0];
                        _nat02Context.EoiProjectsFinished.Add(finished);

                        SendProjectCompletedEmailToCSR(_CSRs, projectNumber, projectRevNumber);
                    }

                    _projectsContext.SaveChanges();
                    _driveworksContext.SaveChanges();
                    _nat02Context.SaveChanges();
                }
                else if (projectType == "TOOLS")
                {
                    EngineeringProjects engineeringProject = _projectsContext.EngineeringProjects.First(p => p.ProjectNumber == projectNumber && p.RevNumber == projectRevNumber);
                    engineeringProject.ToolChecked = true;
                    engineeringProject.ToolCheckedDateTime = DateTime.UtcNow;
                    engineeringProject.ToolCheckedBy = user.GetDWPrincipalId();
                    _projectsContext.SaveChanges();
                    _projectsContext.Remove(engineeringProject);
                    _projectsContext.SaveChanges();
                    EngineeringArchivedProjects engineeringArchivedProject = _projectsContext.EngineeringArchivedProjects.First(p => p.ProjectNumber == projectNumber && p.RevNumber == projectRevNumber);
                    engineeringArchivedProject.ArchivedFromCheck = true;
                    engineeringArchivedProject.ArchivedBy = user.GetDWPrincipalId();

                    List<string> _CSRs = new List<string>();

                    if (!string.IsNullOrEmpty(_projectsContext.EngineeringProjects.First(p => p.ProjectNumber == projectNumber && p.RevNumber == projectRevNumber).CSR))
                    {
                        _CSRs.Add(_projectsContext.EngineeringProjects.First(p => p.ProjectNumber == projectNumber && p.RevNumber == projectRevNumber).CSR);
                    }
                    if (!string.IsNullOrEmpty(_projectsContext.EngineeringProjects.First(p => p.ProjectNumber == projectNumber && p.RevNumber == projectRevNumber).ReturnToCSR))
                    {
                        _CSRs.Add(_projectsContext.EngineeringProjects.First(p => p.ProjectNumber == projectNumber && p.RevNumber == projectRevNumber).ReturnToCSR);
                    }


                    EoiProjectsFinished finished = new EoiProjectsFinished();
                    finished.ProjectNumber = int.Parse(projectNumber);
                    finished.RevisionNumber = int.Parse(projectRevNumber);
                    finished.Csr = _CSRs[0];
                    _nat02Context.EoiProjectsFinished.Add(finished);

                    SendProjectCompletedEmailToCSR(_CSRs, projectNumber, projectRevNumber);

                    _projectsContext.SaveChanges();
                    _driveworksContext.SaveChanges();
                    _nat02Context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            _projectsContext.Dispose();
            _driveworksContext.Dispose();
            _nat02Context.Dispose();
        }
        /// <summary>
        /// Puts on Hold an EngineeringProject based on input projectType "TABLETS" or "TOOLS".
        /// </summary>
        /// <param name="projectNumber"></param>
        /// <param name="projectRevNumber"></param>
        public static void TakeProjectOffHold(string projectNumber, string projectRevNumber)
        {
            using var _projectsContext = new ProjectsContext();
            try
            {
                EngineeringProjects engineeringProject = _projectsContext.EngineeringProjects.First(p => p.ProjectNumber == projectNumber && p.RevNumber == projectRevNumber);
                engineeringProject.OnHold = false;
                engineeringProject.OnHoldComment = "";
                engineeringProject.OnHoldDateTime = null;
                _projectsContext.SaveChanges();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            _projectsContext.Dispose();
        }
        /// <summary>
        /// Cancels an EngineeringProject based on input projectType "TABLETS" or "TOOLS".
        /// </summary>
        /// <param name="projectNumber"></param>
        /// <param name="projectRevNumber"></param>
        /// <param name="user"></param>
        public static void CancelProject(string projectNumber, string projectRevNumber, User user)
        {
            MessageBoxResult res = MessageBox.Show("Are you sure you want to cancel project# " + projectNumber + "_" + projectRevNumber + "?", "Cancel Project", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.Yes)
            {
                using var _projectsContext = new ProjectsContext();
                using var _driveworksContext = new DriveWorksContext();
                try
                {
                    EngineeringProjects engineeringProject = _projectsContext.EngineeringProjects.First(p => p.ProjectNumber == projectNumber && p.RevNumber == projectRevNumber);
                    _projectsContext.Remove(engineeringProject);
                    _projectsContext.SaveChanges();
                    while (!_projectsContext.EngineeringArchivedProjects.Any(p => p.ProjectNumber == projectNumber && p.RevNumber == projectRevNumber))
                    {
                        System.Threading.Thread.Sleep(500);
                    }
                    EngineeringArchivedProjects engineeringArchivedProject = _projectsContext.EngineeringArchivedProjects.First(p => p.ProjectNumber == projectNumber && p.RevNumber == projectRevNumber);
                    engineeringArchivedProject.ArchivedFromCancel = true;
                    engineeringArchivedProject.ArchivedBy = user.GetDWPrincipalId();

                    _projectsContext.SaveChanges();
                    _driveworksContext.SaveChanges();

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                _projectsContext.Dispose();
                _driveworksContext.Dispose();
            }
        }
        /// <summary>
        /// Takes List<string> of Driveworks.SecurityUsers.PrincipalId's for to, cc, and bcc
        /// </summary>
        /// <param name="to"></param>
        /// <param name="cc"></param>
        /// <param name="bcc"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="priority"></param>
        public static void SendEmail(List<string> to, List<string> cc = null, List<string> bcc = null, string subject = "", string body = "", List<string> attachments = null, MailPriority priority = MailPriority.Normal)
        {
            SmtpClient smtpServer = new SmtpClient();
            MailMessage mail = new MailMessage();
            try
            {
                smtpServer.Port = 25;
                smtpServer.Host = "192.168.1.186";
                mail.IsBodyHtml = true;
                mail.From = new MailAddress("AutomatedEmail@natoli.com");
                if (to != null)
                {
                    foreach (string recipient in to)
                    {
                        mail.To.Add(GetEmailAddressFromDWFirstName(recipient));
                    }
                }
                if (cc != null)
                {
                    foreach (string recipient in cc)
                    {
                        mail.CC.Add(GetEmailAddressFromDWFirstName(recipient));
                    }
                }
                if (bcc != null)
                {
                    foreach (string recipient in bcc)
                    {
                        mail.Bcc.Add(GetEmailAddressFromDWFirstName(recipient));
                    }
                }
                if (attachments != null)
                {
                    foreach (string path in attachments)
                    {
                        Attachment attachment = new Attachment(path);
                        mail.Attachments.Add(attachment);
                    }
                }
                mail.Subject = subject;
                mail.Body = body;
                mail.Priority = priority;
                smtpServer.Send(mail);
                smtpServer.Dispose();
                mail.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            smtpServer.Dispose();
            mail.Dispose();
        }
        /// <summary>
        /// Checks for processName in processes and brings to front.
        /// </summary>
        /// <param name="processName"></param>
        public static void BringProcessToFront(string processName)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            try
            {
                // Get process Name
                process = System.Diagnostics.Process.GetProcessesByName(processName)[0];
            }
            catch
            {
                process.Dispose();
                return;
            }

            // Get a handle of the process.
            IntPtr handle = NativeMethods.FindWindow(null, process.MainWindowTitle);

            // Verify that it is a running process.
            if (handle == IntPtr.Zero)
            {
                process.Dispose();
                return;
            }

            // Make it the foreground application
            NativeMethods.SetForegroundWindow(handle);
            process.Dispose();
        }
        /// <summary>
        /// Returns E-mail Address from Driveworks.SecurityUsers.DisplayName
        /// </summary>
        /// <param name="displayName"></param>
        /// <returns></returns>
        public static string GetEmailAddress(string displayName)
        {
            try
            {
                switch (displayName)
                {
                    case "GREGORY":
                        displayName = "Greg";
                        return "intlcs1@natoli.com";
                    case "NICHOLAS":
                        displayName = "Nick";
                        return "intlcs1@natoli.com";
                    default:
                        break;
                }
                using var _driveworksContext = new DriveWorksContext();
                return _driveworksContext.SecurityUsers.Where(u => u.DisplayName.Contains(displayName)).FirstOrDefault().EmailAddress;
            }
            catch (Exception eSql)
            {
                MessageBox.Show("Error resolving email address.\n" + eSql.Message);
                return null;
            }
        }
        /// <summary>
        /// Takes the PrincipalId of the Driveworks.SecurityUsers table and returns the EmailAddress.
        /// </summary>
        /// <param name="dWfirstName"></param>
        /// <returns></returns>
        public static string GetEmailAddressFromDWFirstName(string dWfirstName)
        {
            try
            {
                using var _driveworksContext = new DriveWorksContext();
                switch (dWfirstName)
                {
                    case "Greg":
                        _driveworksContext.Dispose();
                        return "intlcs1@natoli.com";
                    case "Nick":
                        _driveworksContext.Dispose();
                        return "intlcs3@natoli.com";
                    default:
                        if (_driveworksContext.SecurityUsers.Any(su => su.PrincipalId.Trim() == dWfirstName.Trim()))
                        {
                            string fullName = _driveworksContext.SecurityUsers.First(su => su.PrincipalId.Trim() == dWfirstName.Trim()).EmailAddress.Trim();
                            _driveworksContext.Dispose();
                            return fullName;
                        }
                        else
                        {
                            _driveworksContext.Dispose();
                            return "";
                        }
                }
            }
            catch //(Exception ex)
            {
                return "";
                //MessageBox.Show(ex.Message);
            }

        }
        /// <summary>
        /// Creates Zip File from 'inputDirectory' and places it at the 'outputZipFile' path.
        /// </summary>
        /// <param name="inputDirectory"></param>
        /// <param name="outputZipFile"></param>
        public static void CreateZipFile(string inputDirectory, string outputZipFile)
        {
            System.IO.Compression.ZipFile.CreateFromDirectory(inputDirectory, outputZipFile, 0, false);
        }
        /// <summary>
        /// Sends project completed E-mail to CSRs
        /// </summary>
        /// <param name="CSRs"></param>
        /// <param name="_projectNumber"></param>
        /// <param name="_revNo"></param>
        public static void SendProjectCompletedEmailToCSR(List<string> CSRs, string _projectNumber, string _revNo)
        {
            SmtpClient smtpServer = new SmtpClient();
            MailMessage mail = new MailMessage();
            try
            {
                // Send email
                smtpServer.Port = 25;
                smtpServer.Host = "192.168.1.186";
                mail.IsBodyHtml = true;
                mail.From = new MailAddress("AutomatedEmail@natoli.com");
                if (CSRs != null)
                {
                    foreach (string CSR in CSRs)
                    {
                        mail.To.Add(GetEmailAddressFromDWFirstName(CSR));
                    }
                    //mail.Bcc.Add("eng6@natoli.com");
                    //mail.Bcc.Add("eng5@natoli.com");
                    string projectNumber = _projectNumber ?? "";
                    string revNo = _revNo ?? "";
                    mail.Subject = "Project# " + projectNumber.Trim() + "-" + revNo.Trim() + " Completed";
                    string filesForCustomerDirectory = @"\\engserver\workstations\TOOLING AUTOMATION\Project Specifications\" + projectNumber + @"\FILES_FOR_CUSTOMER\";
                    string zipFile = @"\\engserver\workstations\TOOLING AUTOMATION\Project Specifications\" + projectNumber + @"\FILES_FOR_CUSTOMER.zip";
                    if (System.IO.Directory.Exists(filesForCustomerDirectory))
                    {
                        CreateZipFile(filesForCustomerDirectory, zipFile);
                        //string[] files = System.IO.Directory.GetFiles(filesForCustomerDirectory);

                        //foreach (string file in files)
                        //{
                        //    string fileName = file.Substring(file.LastIndexOf(@"\") + 1, file.Length - file.LastIndexOf(@"\") - 1);
                        //    Attachment attachment = new Attachment(file);
                        //    mail.Attachments.Add(attachment);
                        //}

                        Attachment attachment = new Attachment(zipFile);
                        mail.Attachments.Add(attachment);
                    }
                    mail.IsBodyHtml = true;
                    mail.Body = "Dear " + CSRs.First() + ",<br><br>" +

                    @"Project# <a href=&quot;\\engserver\workstations\TOOLING%20AUTOMATION\Project%20Specifications\" + projectNumber + @"\&quot;>" + projectNumber + " </a> is completed and ready to be viewed.<br> " +
                    "The drawings for the customer are attached.<br><br>" +
                    "Thanks,<br>" +
                    "Engineering Team<br><br><br>" +


                    "This is an automated email and not monitored by any person(s).";
                    smtpServer.Send(mail);
                    smtpServer.Dispose();
                    mail.Dispose();
                    System.IO.File.Delete(zipFile);
                    //MessageBox.Show("Message sent to CSR.");
                }
                else
                {
                    MessageBox.Show("List of strings 'CSRs' was null.");
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
                // WriteToErrorLog("SendEmailToCSR", ex.Message);
            }
            smtpServer.Dispose();
            mail.Dispose();
        }
        /// <summary>
        /// Returns a list of errors found for specified quote and rev level.
        /// </summary>
        /// <param name="quoteNo"></param>
        /// <param name="quoteRevNo"></param>
        /// <returns></returns>
        public static List<string> QuoteErrors(string quoteNo, string quoteRevNo)
        {
            using var _nat01Context = new NAT01Context();
            using var _driveworksContext = new DriveWorksContext();
            Quote quote = new Quote(Convert.ToInt32(quoteNo), Convert.ToInt16(quoteRevNo));
            List<QuoteDetails> quoteDetails = quote.Nat01Context.QuoteDetails.Where(l => (int)l.QuoteNo == Convert.ToInt32(quoteNo) && l.Revision == Convert.ToInt16(quoteRevNo)).OrderBy(q => q.LineNumber).ToList();
            QuoteLineItem[] quoteLineItems = null;
            foreach (QuoteDetails line in quoteDetails)
            {
                quoteLineItems.Append(new QuoteLineItem(quote, line.LineNumber));
            }
            List<string> errors = new List<string>();

            if (quoteLineItems.Length > 0)
            {
                // Shape Descriptions
                if (quoteLineItems.Length > 1)
                {
                    string shapeDescription = "";
                    foreach (QuoteLineItem quoteLineItem in quoteLineItems)
                    {
                        if (!string.IsNullOrEmpty(shapeDescription) && !string.IsNullOrEmpty(quoteLineItem.HobNoShapeID) && (_nat01Context.HobList.Any(h => h.HobNo == quoteLineItem.HobNoShapeID && h.TipQty == (quoteLineItem.TipQTY ?? 1) && h.BoreCircle == (quoteLineItem.BoreCircle ?? 0)) || _nat01Context.DieList.Any(d => d.DieId == quoteLineItem.HobNoShapeID)) && (string.IsNullOrEmpty(quoteLineItem.Desc2) || quoteLineItem.Desc2.Trim() != shapeDescription.Trim()))
                        {
                            errors.Add("Shape Descriptions '" + shapeDescription + "' and '" + (string.IsNullOrEmpty(quoteLineItem.Desc2) ? "NULL" : quoteLineItem.Desc2.Trim()) + "' do not match.");
                            break;
                        }
                        shapeDescription = quoteLineItem.Desc2.Trim();
                    }
                }

                // Machine Not Set Up For End User
                if (!_nat01Context.CustomerMachines.Any(m => m.CustomerNo.Trim() == quote.UserAcctNo && m.CustAddressCode.Trim() == quote.UserLocNo && m.MachineNo == quoteLineItems.First(ql => ql.MachineNo != null && ql.MachineNo > 0).MachineNo))
                {
                    errors.Add("Machine " + quoteLineItems.First(ql => ql.MachineNo != null && ql.MachineNo > 0).MachineNo + " not setup for " + quote.UserAcctNo + " - " + quote.UserLocNo + ".");
                }

                // Tablet is Too Large for press
                if (quoteLineItems.Any(qli=>qli.MachineNo!= null && qli.MachineNo>0))
                {
                    if (_nat01Context.MachineList.Any(m => m.MachineNo == quoteLineItems.First(qli => qli.MachineNo != null && qli.MachineNo > 0).MachineNo))
                    {
                        MachineList machine = _nat01Context.MachineList.First(m => m.MachineNo == quoteLineItems.First(qli => qli.MachineNo != null && qli.MachineNo > 0).MachineNo);
                        // Is B Machine
                        if ((machine.UpperSize ?? machine.LowerSize) != @"3/4 x 5-3/4" && (machine.MachineTypePrCode.Trim() == "B" || machine.MachineTypePrCode.Trim() == "BB" || machine.MachineTypePrCode.Trim() == "BBS" ||
                                           ((machine.MachineTypePrCode.Trim() == "ZZZ" || machine.MachineTypePrCode.Trim() == "DRY") && (machine.UpperSize ?? machine.LowerSize) == @"1 x 5-3/4"))
                                            )
                        {
                            // Has A Hob W/ a Die ID of width or length > .75
                            if (quoteLineItems.Any(qli => qli.LineItemType != "D" && qli.LineItemType != "DS" && qli.LineItemType != "DA" && qli.LineItemType != "DC" && qli.LineItemType != "DI" && qli.LineItemType != "DP" &&
                             _nat01Context.HobList.Any(h => h.HobNo == qli.HobNoShapeID && h.TipQty == (qli.TipQTY ?? 1) && h.BoreCircle == (qli.BoreCircle ?? 0)) && _nat01Context.DieList.Any(d => !string.IsNullOrEmpty(d.DieId) && d.DieId.Trim() == _nat01Context.HobList.First(h => h.HobNo == qli.HobNoShapeID && h.TipQty == (qli.TipQTY ?? 1) && h.BoreCircle == (qli.BoreCircle ?? 0)).DieId.Trim() && (d.LengthMajorAxis > .75 || d.WidthMinorAxis > .75))))
                            {
                                errors.Add("Tablet is too large for press.");
                            }
                            // Has a Die W/ Die ID of width or length > .75
                            else if (quoteLineItems.Any(qli => qli.LineItemType == "D" && qli.LineItemType == "DS" && qli.LineItemType == "DA" && qli.LineItemType == "DC" && qli.LineItemType == "DI" && qli.LineItemType == "DP" &&
                             _nat01Context.DieList.Any(d => !string.IsNullOrEmpty(d.DieId) && d.DieId.Trim() == qli.HobNoShapeID.Trim() && (d.WidthMinorAxis > .75 || d.LengthMajorAxis > .75))))
                            {
                                errors.Add("Tablet is too large for press.");
                            }
                        }
                        // Is D Machine
                        if (machine.MachineTypePrCode.Trim() == "D" ||
                            ((machine.MachineTypePrCode.Trim() == "ZZZ" || machine.MachineTypePrCode.Trim() == "DRY") && (machine.UpperSize ?? machine.LowerSize) == @"1-1/4 x 5-3/4") ||
                            machine.MachineNo == 1015)
                        {
                            // Has A Hob W/ a Die ID of width or length > 1.0
                            if (quoteLineItems.Any(qli => qli.LineItemType != "D" && qli.LineItemType != "DS" && qli.LineItemType != "DA" && qli.LineItemType != "DC" && qli.LineItemType != "DI" && qli.LineItemType != "DP" &&
                             _nat01Context.HobList.Any(h => h.HobNo == qli.HobNoShapeID && h.TipQty == (qli.TipQTY ?? 1) && h.BoreCircle == (qli.BoreCircle ?? 0)) && _nat01Context.DieList.Any(d => !string.IsNullOrEmpty(d.DieId) && d.DieId.Trim() == _nat01Context.HobList.First(h => h.HobNo == qli.HobNoShapeID && h.TipQty == (qli.TipQTY ?? 1) && h.BoreCircle == (qli.BoreCircle ?? 0)).DieId.Trim() && (d.LengthMajorAxis > 1.0 || d.WidthMinorAxis > 1.0))))
                            {
                                errors.Add("Tablet is too large for press.");
                            }
                            // Has a Die W/ Die ID of width or length > 1.0
                            else if (quoteLineItems.Any(qli => qli.LineItemType == "D" && qli.LineItemType == "DS" && qli.LineItemType == "DA" && qli.LineItemType == "DC" && qli.LineItemType == "DI" && qli.LineItemType == "DP" &&
                             _nat01Context.DieList.Any(d => !string.IsNullOrEmpty(d.DieId) && d.DieId.Trim() == qli.HobNoShapeID.Trim() && (d.WidthMinorAxis > 1.0 || d.LengthMajorAxis > 1.0))))
                            {
                                errors.Add("Tablet is too large for press.");
                            }
                        }
                    }
                }


                string workingLengthTolerance = null;
                bool varyingWLTolerances = false;
                string machineDescription = null;
                foreach (QuoteLineItem quoteLineItem in quoteLineItems)
                {
                    if (quoteLineItem.MachineNo != null && quoteLineItem.MachineNo > 0)
                    {
                        // Machine description is NOT empty
                        if (!string.IsNullOrEmpty(quoteLineItem.MachineDescription))
                        {
                            // First Description
                            if (machineDescription == null)
                            {
                                machineDescription = quoteLineItem.MachineDescription.Trim();
                            }
                            else
                            {
                                // Descriptions do not match
                                if (machineDescription != quoteLineItem.MachineDescription.Trim())
                                {
                                    errors.Add("A machine description does not match '" + quoteLineItem.LineItemType + "'s machine description.");
                                }
                                machineDescription = quoteLineItem.MachineDescription.Trim();
                            }
                        }
                        else
                        {
                            errors.Add("'" + quoteLineItem.LineItemType + "' does not have a machine description.");
                        }
                    }
                    // Upper, Lower, Reject
                    if (quoteLineItem.LineItemType == "U" ||
                        quoteLineItem.LineItemType == "L" || quoteLineItem.LineItemType == "LCRP" ||
                        quoteLineItem.LineItemType == "R")
                    {
                        // Upper
                        if (quoteLineItem.LineItemType == "U")
                        {
                            // Machine Exists
                            if (_nat01Context.MachineList.Any(m => quoteLineItem.MachineNo != null && quoteLineItem.MachineNo > 0 && m.MachineNo == quoteLineItem.MachineNo))
                            {
                                MachineList machine = _nat01Context.MachineList.First(m => quoteLineItem.MachineNo != null && quoteLineItem.MachineNo > 0 && m.MachineNo == quoteLineItem.MachineNo);
                                // Is B or D Machine
                                if (((machine.UpperSize ?? machine.LowerSize) != @"3/4 x 5-3/4" &&( machine.MachineTypePrCode.Trim() == "B" || machine.MachineTypePrCode.Trim() == "BB" || machine.MachineTypePrCode.Trim() == "BBS" ||
                                    ((machine.MachineTypePrCode.Trim() == "ZZZ" || machine.MachineTypePrCode.Trim() == "DRY") && (machine.UpperSize ?? machine.LowerSize) == @"1 x 5-3/4"))) 
                                    || 
                                    (machine.MachineTypePrCode.Trim() == "D" ||
                                        ((machine.MachineTypePrCode.Trim() == "ZZZ" || machine.MachineTypePrCode.Trim() == "DRY") && (machine.UpperSize ?? machine.LowerSize) == @"1-1/4 x 5-3/4") ||
                                        machine.MachineNo == 1015))
                                {
                                    // Is NOT Apotex
                                    if (!(quote.UserAcctNo == "1031250" || quote.UserAcctNo == "1001400"))
                                    {
                                        // Machine requires tip length for oil seals
                                        if (ContainsAny(machine.Description,new List<string> { "FETTE", "KILIAN", "KORSCH", "X-PRESS" ,"XS-PRESS"}, StringComparison.InvariantCultureIgnoreCase) || ( machine.Description.Contains("GENESIS",StringComparison.InvariantCultureIgnoreCase) && machine.Description.Contains("STOKES", StringComparison.InvariantCultureIgnoreCase)) || machine.SpecialInfo.Contains("102") )
                                        {
                                            // Does not have option 102
                                            if (!quoteLineItem.OptionNumbers.Contains("102"))
                                            {
                                                errors.Add("'U' may require tip length for oil seals.");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (!varyingWLTolerances)
                        {
                            if (quoteLineItem.OptionNumbers.Contains("336"))
                            {
                                if (workingLengthTolerance == null)
                                {
                                    workingLengthTolerance = string.Join(string.Empty, quoteLineItem.Options[336]);
                                }
                                else if (workingLengthTolerance != string.Join(string.Empty, quoteLineItem.Options[336]))
                                {
                                    errors.Add("Working Length Tolerances vary. Check to make sure they contain correct values.");
                                }
                            }
                            else
                            {
                                if (workingLengthTolerance != null)
                                {
                                    errors.Add("Working Length Tolerances vary. Check to make sure they contain correct values.");
                                }
                            }
                        }
                    }
                    // Die
                    if (quoteLineItem.LineItemType == "D")
                    {
                        // No Die Groove
                        if (!ContainsAny(string.Join(string.Empty, quoteLineItem.OptionNumbers), new List<string> { "430", "431", "432", "433", "434", "435", "436", "437", "438", "439" }, StringComparison.CurrentCulture))
                        {
                            errors.Add("No die groove on 'D'");
                        }
                        // Too Many Die Grooves
                        if (!quoteLineItem.OptionNumbers.Contains("435") && quoteLineItem.OptionNumbers.Count(o => ContainsAny(o, new List<string> { "430", "431", "432", "433", "434", "435", "436", "437", "438", "439" }, StringComparison.CurrentCulture)) > 1)
                        {
                            errors.Add("'D' has too many die grooves.");
                        }
                        // Not appropriate interchangeability options
                        if (quoteLineItems.Any(qli => (qli.LineItemType == "A" ||
                        ((qli.LineItemType == "U" || qli.LineItemType == "UH") && qli.OptionNumbers.Contains("160")) ||
                        ((qli.LineItemType == "R" || qli.LineItemType == "RH") && qli.OptionNumbers.Contains("160"))) && !quoteLineItem.OptionNumbers.Contains("422")))
                        {
                            errors.Add("'D' needs (422) SPECIAL BORE CONCENTRICITY for 100% interchangeability");
                        }

                        // Die Number Exists
                        if (_nat01Context.DieList.Any(d => !string.IsNullOrEmpty(d.DieId) && d.DieId.Trim() == quoteLineItem.HobNoShapeID))
                        {
                            DieList die = _nat01Context.DieList.First(d => !string.IsNullOrEmpty(d.DieId) && d.DieId.Trim() == quoteLineItem.HobNoShapeID);
                            // Machine Number Exists
                            if (_nat01Context.MachineList.Any(m => quoteLineItem.MachineNo != null && quoteLineItem.MachineNo > 0 && m.MachineNo == quoteLineItem.MachineNo))
                            {
                                MachineList machine = _nat01Context.MachineList.First(m => quoteLineItem.MachineNo != null && quoteLineItem.MachineNo > 0 && m.MachineNo == quoteLineItem.MachineNo);
                                // Is in shallow groove range
                                if(machine.Od<.946 && die.LengthMajorAxis>.625)
                                {
                                    // Does not have Shallow Groove and A2 Steel
                                    if (!quoteLineItem.OptionNumbers.Contains("430") && !quoteLineItem.Material.Contains("A2 "))
                                    {
                                        errors.Add("'D' has large bore. Consider adding shallow groove and A2 steel.");
                                    }
                                    else //Has at least 1 of above
                                    {
                                        // Does not have Shallow Groove
                                        if (!quoteLineItem.OptionNumbers.Contains("430"))
                                        {
                                            errors.Add("'D' has large bore. Consider adding a shallow groove.");
                                        }
                                        // Does not have A2 Steel
                                        if (!quoteLineItem.Material.Contains("A2 "))
                                        {
                                            errors.Add("'D' has large bore. Consider changing to A2 steel.");
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            errors.Add("'D' has incorrect Die Number");
                        }

                        // Has Inserts
                        if (ContainsAny(string.Join(string.Empty, quoteLineItem.OptionNumbers), new List<string> { "491", "492", "493", "494", "495", "496" }, StringComparison.CurrentCulture))
                        {
                            // Die Groove W/O Relief
                            if (quoteLineItem.OptionNumbers.Contains("437"))
                            {
                                errors.Add("'D' cannot have groove without relief and an insert.");
                            }

                            // Carbide || Ceramic
                            if (ContainsAny(string.Join(string.Empty, quoteLineItem.OptionNumbers), new List<string> { "491", "492", "496" }, StringComparison.CurrentCulture))
                            {
                                // Is NOT Taper 2X
                                if (quoteLineItem.OptionNumbers.Contains("391"))
                                {
                                    // Carbide tip || Ceramic tip
                                    if (quoteLineItems.Any(qli => ContainsAny(string.Join(string.Empty, qli.OptionNumbers), new List<string> { "240", "250" }, StringComparison.CurrentCulture)))
                                    {
                                        errors.Add("If both tips and die are carbide/ceramic, TX2 must be added to the die.");
                                    }
                                }
                                
                            }
                        }
                    }


                    // Punches, Holders, and Assemblies
                    if (quoteLineItem.LineItemType == "U" || quoteLineItem.LineItemType == "UH" || quoteLineItem.LineItemType == "UA" ||
                        quoteLineItem.LineItemType == "L" || quoteLineItem.LineItemType == "LH" || quoteLineItem.LineItemType == "LCRP" || quoteLineItem.LineItemType == "LA" ||
                        quoteLineItem.LineItemType == "R" || quoteLineItem.LineItemType == "RH" || quoteLineItem.LineItemType == "RA")
                    {
                        // Upper or Upper Assembly
                        if (quoteLineItem.LineItemType == "U" || quoteLineItem.LineItemType == "UA")
                        {
                            // Natoli Deep Fill
                            if (quoteLineItem.MachineDescription.Contains("DEEP") && quoteLineItem.MachineDescription.Contains("FILL") && quoteLineItem.MachineDescription.Contains("NATOLI"))
                            {
                                // Special tip straight is not .75"
                                if (!_nat01Context.QuoteOptionValueASingleNum.Any(ov=> ov.QuoteNo == Convert.ToInt32(quoteNo) && ov.RevNo == Convert.ToInt16(quoteRevNo) && (ov.QuoteDetailType=="U" || ov.QuoteDetailType=="UA") && ov.OptionCode=="217" && Math.Round((decimal)ov.Number1,3)==(decimal).750))
                                {
                                    errors.Add("Upper or Upper Assembly should have a tip straight of .75\"");
                                }
                            }
                        }
                        // No Key Angle
                        if (ContainsAny(string.Join(string.Empty, quoteLineItem.OptionNumbers), new List<string> { "130", "131", "132", "133", "139", "140", "141", "144" }, StringComparison.CurrentCulture) &&
                            !ContainsAny(string.Join(string.Empty, quoteLineItem.OptionNumbers), new List<string> { "155", "156" }, StringComparison.CurrentCulture))
                        {
                            errors.Add("'" + quoteLineItem.LineItemType + "' is missing a key angle.");
                        }
                    }

                    // Punches, Holders, and Heads
                    if (quoteLineItem.LineItemType == "U" || quoteLineItem.LineItemType == "UH" || quoteLineItem.LineItemType == "UHD" ||
                        quoteLineItem.LineItemType == "L" || quoteLineItem.LineItemType == "LH" || quoteLineItem.LineItemType == "LCRP" || quoteLineItem.LineItemType == "LHD" ||
                        quoteLineItem.LineItemType == "R" || quoteLineItem.LineItemType == "RH" || quoteLineItem.LineItemType == "RHD")
                    {
                        // No Head Option
                        if (!ContainsAny(string.Join(string.Empty, quoteLineItem.OptionNumbers), new List<string> { "001", "002", "003", "004", "005", "006", "007", "008", "009", "010", "011", "012", "013", "014", "015", "016", "014", "015", "016", "018", "019", "022", "024", "025" }, StringComparison.CurrentCulture))
                        {
                            errors.Add("'" + quoteLineItem.LineItemType + "' is missing a head type option.");
                        }

                        // Machine Exists
                        if (_nat01Context.MachineList.Any(m => quoteLineItem.MachineNo != null && quoteLineItem.MachineNo > 0 && m.MachineNo == quoteLineItem.MachineNo))
                        {
                            MachineList machine = _nat01Context.MachineList.First(m => quoteLineItem.MachineNo != null && quoteLineItem.MachineNo > 0 && m.MachineNo == quoteLineItem.MachineNo);
                            // Machine is D and NOT EU1-441
                            if ((machine.MachineTypePrCode.Trim() == "D" || ((machine.MachineTypePrCode.Trim() == "ZZZ" || machine.MachineTypePrCode.Trim() == "DRY") && (machine.UpperSize ?? machine.LowerSize) == @"1-1/4 x 5-3/4") || machine.MachineNo == 1015) &&
                                (machine.UpperSize ?? machine.LowerSize) != "1-1/2 x 5-3/4")
                            {
                                // Has 441 style head option
                                if (ContainsAny(string.Join(string.Empty, quoteLineItem.OptionNumbers), new List<string> { "007", "011", "013", "014", "018" }, StringComparison.CurrentCulture))
                                {
                                    errors.Add("'" + quoteLineItem.LineItemType + "' has a 441 style head but the machine is a normal 'D' machine.");
                                }
                            }
                            // Machine is D and EU1-441
                            else if ((machine.MachineTypePrCode.Trim() == "D" || ((machine.MachineTypePrCode.Trim() == "ZZZ" || machine.MachineTypePrCode.Trim() == "DRY") && (machine.UpperSize ?? machine.LowerSize) == @"1-1/4 x 5-3/4") || machine.MachineNo == 1015) &&
                                    (machine.UpperSize ?? machine.LowerSize) == "1-1/2 x 5-3/4")
                            {
                                // Does not have 441 head option or special
                                if (!ContainsAny(string.Join(string.Empty, quoteLineItem.OptionNumbers), new List<string> { "007", "008", "011", "013", "014", "018" }, StringComparison.CurrentCulture))
                                {
                                    errors.Add("'" + quoteLineItem.LineItemType + "' does not have a 441 style head but the machine is of 441 type.");
                                }
                            }

                            // Machine is B and NOT FS19
                            if (((machine.UpperSize ?? machine.LowerSize) != @"3/4 x 5-3/4" && (machine.MachineTypePrCode.Trim() == "B" || machine.MachineTypePrCode.Trim() == "BB" || machine.MachineTypePrCode.Trim() == "BBS" || ((machine.MachineTypePrCode.Trim() == "ZZZ" || machine.MachineTypePrCode.Trim() == "DRY") && (machine.UpperSize ?? machine.LowerSize) == @"1 x 5-3/4"))) &&
                                (machine.UpperSize ?? machine.LowerSize) != "1-1/4 x 5-3/4")
                            {
                                // Has FS-19 style head option
                                if (ContainsAny(string.Join(string.Empty, quoteLineItem.OptionNumbers), new List<string> { "016", "019" }, StringComparison.CurrentCulture))
                                {
                                    errors.Add("'" + quoteLineItem.LineItemType + "' has an FS-19 style head but the machine is a normal 'B' machine.");
                                }
                            }
                            // Machine is B and FS-19
                            else if (((machine.UpperSize ?? machine.LowerSize) != @"3/4 x 5-3/4" && (machine.MachineTypePrCode.Trim() == "B" || machine.MachineTypePrCode.Trim() == "BB" || machine.MachineTypePrCode.Trim() == "BBS" || ((machine.MachineTypePrCode.Trim() == "ZZZ" || machine.MachineTypePrCode.Trim() == "DRY") && (machine.UpperSize ?? machine.LowerSize) == @"1 x 5-3/4"))) &&
                                (machine.UpperSize ?? machine.LowerSize) != "1-1/4 x 5-3/4")
                            {
                                // Does not have FS-19 head option or special
                                if (!ContainsAny(string.Join(string.Empty, quoteLineItem.OptionNumbers), new List<string> { "008", "016", "019" }, StringComparison.CurrentCulture))
                                {
                                    errors.Add("'" + quoteLineItem.LineItemType + "' does not have an FS-19 style head but the machine is of FS-19 type.");
                                }
                            }

                            // Machine is B and NOT FS-12
                            if (((machine.UpperSize ?? machine.LowerSize) != @"3/4 x 5-3/4" && (machine.MachineTypePrCode.Trim() == "B" || machine.MachineTypePrCode.Trim() == "BB" || machine.MachineTypePrCode.Trim() == "BBS" || ((machine.MachineTypePrCode.Trim() == "ZZZ" || machine.MachineTypePrCode.Trim() == "DRY") && (machine.UpperSize ?? machine.LowerSize) == @"1 x 5-3/4"))))
                            {
                                // Has FS-12 style head option
                                if (ContainsAny(string.Join(string.Empty, quoteLineItem.OptionNumbers), new List<string> { "017"}, StringComparison.CurrentCulture))
                                {
                                    errors.Add("'" + quoteLineItem.LineItemType + "' has an FS-12 style head but the machine is a normal 'B' machine.");
                                }
                            }
                            // Machine is B and FS-12
                            else if (((machine.UpperSize ?? machine.LowerSize) == @"3/4 x 5-3/4" && (machine.MachineTypePrCode.Trim() == "B" || machine.MachineTypePrCode.Trim() == "BB" || machine.MachineTypePrCode.Trim() == "BBS" || ((machine.MachineTypePrCode.Trim() == "ZZZ" || machine.MachineTypePrCode.Trim() == "DRY") && (machine.UpperSize ?? machine.LowerSize) == @"1 x 5-3/4"))))
                            {
                                // Does not have FS-12 head option or special
                                if (!ContainsAny(string.Join(string.Empty, quoteLineItem.OptionNumbers), new List<string> { "008", "017" }, StringComparison.CurrentCulture))
                                {
                                    errors.Add("'" + quoteLineItem.LineItemType + "' does not have an FS-12 style head but the machine is of FS-12 type.");
                                }
                            }
                        }

                    }

                    // Punches and Holders
                    if (quoteLineItem.LineItemType == "U" || quoteLineItem.LineItemType == "UH" ||
                        quoteLineItem.LineItemType == "L" || quoteLineItem.LineItemType == "LH" || quoteLineItem.LineItemType == "LCRP" ||
                        quoteLineItem.LineItemType == "R" || quoteLineItem.LineItemType == "RH")
                    {
                        // Machine Exists
                        if (_nat01Context.MachineList.Any(m => quoteLineItem.MachineNo != null && quoteLineItem.MachineNo > 0 && m.MachineNo == quoteLineItem.MachineNo))
                        {
                            MachineList machine = _nat01Context.MachineList.First(m => quoteLineItem.MachineNo != null && quoteLineItem.MachineNo > 0 && m.MachineNo == quoteLineItem.MachineNo);
                            // Is B or D Machine
                            if (((machine.UpperSize ?? machine.LowerSize) != @"3/4 x 5-3/4" && (machine.MachineTypePrCode.Trim() == "B" || machine.MachineTypePrCode.Trim() == "BB" || machine.MachineTypePrCode.Trim() == "BBS" ||
                                ((machine.MachineTypePrCode.Trim() == "ZZZ" || machine.MachineTypePrCode.Trim() == "DRY") && (machine.UpperSize ?? machine.LowerSize) == @"1 x 5-3/4")))
                                ||
                                (machine.MachineTypePrCode.Trim() == "D" ||
                                    ((machine.MachineTypePrCode.Trim() == "ZZZ" || machine.MachineTypePrCode.Trim() == "DRY") && (machine.UpperSize ?? machine.LowerSize) == @"1-1/4 x 5-3/4") ||
                                    machine.MachineNo == 1015))
                            {
                                // Fette or Korsch
                                if (ContainsAny(quoteLineItem.MachineDescription, new List<string> { "FETTE", "KORSCH" }))
                                {
                                    // Does not have hold low barrel tolerance
                                    if (!quoteLineItem.OptionNumbers.Contains("123"))
                                    {
                                        // Special barrel diameter
                                        if (quoteLineItem.OptionNumbers.Contains("120"))
                                        {

                                        }
                                        else
                                        {
                                            // Lower
                                            if (quoteLineItem.LineItemType == "L" || quoteLineItem.LineItemType == "LH" || quoteLineItem.LineItemType == "LCRP")
                                            {
                                                bool eu = false;
                                                if (quoteLineItem.OptionNumbers.Contains("116") && machine.Description.ToUpper().Contains("SYNTHESIS"))
                                                {
                                                    eu = true;
                                                }
                                                else
                                                {
                                                    foreach (QuoteLineItem qli in quoteLineItems)
                                                    {
                                                        if (ContainsAny(string.Join(string.Empty, quoteLineItem.OptionNumbers), new List<string> { "333", "003", "004", "005", "006", "007", "009" }, StringComparison.CurrentCulture))
                                                        {
                                                            eu = true;
                                                        }
                                                    }
                                                    // EU Type
                                                    if (eu == true)
                                                    {
                                                        errors.Add("'L' has full barrel diameter. Consider adding hold low barrel diameter tolerance.");
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                errors.Add("Consider adding hold low barrel diameter tolerance to '" + quoteLineItem.LineItemType + "'.");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    // Punches and Tips
                    if (quoteLineItem.LineItemType == "U" || quoteLineItem.LineItemType == "UT" ||
                        quoteLineItem.LineItemType == "L" || quoteLineItem.LineItemType == "LCRP" || quoteLineItem.LineItemType == "LT" ||
                        quoteLineItem.LineItemType == "R" || quoteLineItem.LineItemType == "RT")
                    {

                        // Has HobNo in HobList
                        if (_nat01Context.HobList.Any(h => h.HobNo == quoteLineItem.HobNoShapeID && h.TipQty == (quoteLineItem.TipQTY ?? 1) && h.BoreCircle == (quoteLineItem.BoreCircle ?? 0)))
                        {
                            HobList hob = _nat01Context.HobList.First(h => h.HobNo == quoteLineItem.HobNoShapeID && h.TipQty == (quoteLineItem.TipQTY ?? 1) && h.BoreCircle == (quoteLineItem.BoreCircle ?? 0));

                            // Has DieId in HobList
                            if (!string.IsNullOrEmpty(hob.DieId))
                            {
                                // Wrong Die Number
                                if (quoteLineItem.DieShapeID.ToString() != hob.DieId.Trim())
                                {
                                    errors.Add("'" + quoteLineItem.LineItemType + "' has the wrong die number.");
                                }

                                // DieId in DieList
                                if (_nat01Context.DieList.Any(d => !string.IsNullOrEmpty(d.DieId) && d.DieId.Trim() == hob.DieId.Trim()))
                                {
                                    DieList die = _nat01Context.DieList.First(d => d.DieId.Trim() == hob.DieId.Trim());
                                    // Cup Depth Incorrect
                                    if (hob.CupDepth > ((die.WidthMinorAxis - (hob.Land * 2)) / 2))
                                    {
                                        errors.Add("'" + quoteLineItem.LineItemType + "' - " + hob.HobNo + " has incorrect cup depth in the database (Magic).");
                                    }

                                    // Has Die
                                    if (quoteLineItems.Any(qli => qli.LineItemType == "D" || qli.LineItemType == "DS"))
                                    {
                                        QuoteLineItem dieQuoteLineItem = quoteLineItems.First(qli => qli.LineItemType == "D" || qli.LineItemType == "DS");
                                        
                                        double? punchWidth = null;
                                        double? punchLength = null;
                                        // Special tip size
                                        if (quoteLineItem.OptionNumbers.Contains("200"))
                                        {
                                            punchWidth = _nat01Context.QuoteOptionValueBDoubleNum.First(qov => qov.QuoteNo == Convert.ToInt32(quoteNo) && qov.RevNo == Convert.ToInt16(quoteRevNo) && !string.IsNullOrEmpty(qov.QuoteDetailType) && qov.QuoteDetailType.Trim() == quoteLineItem.LineItemType && qov.OptionCode == "200").Number1;
                                            punchLength = _nat01Context.QuoteOptionValueBDoubleNum.First(qov => qov.QuoteNo == Convert.ToInt32(quoteNo) && qov.RevNo == Convert.ToInt16(quoteRevNo) && !string.IsNullOrEmpty(qov.QuoteDetailType) && qov.QuoteDetailType.Trim() == quoteLineItem.LineItemType && qov.OptionCode == "200").Number2;
                                        }
                                        else
                                        {
                                            // IsRound
                                            if (die.ShapeCode == "1" || die.ShapeCode == "18" || die.ShapeCode == "93")
                                            {
                                                // Is Lower
                                                if (quoteLineItem.LineItemType.Contains("L"))
                                                {
                                                    punchWidth = (double)die.WidthMinorAxis - (double)_driveworksContext.TipClearancesRoundLower.Where(c => c.NominalDiameter < (decimal)die.WidthMinorAxis).Max(c => c.Clearance);
                                                }
                                                else
                                                {
                                                    punchWidth = (double)die.WidthMinorAxis - (double)_driveworksContext.TipClearancesRoundUpper.Where(c => c.NominalDiameter < (decimal)die.WidthMinorAxis).Max(c => c.Clearance);
                                                }
                                                
                                            }
                                            else
                                            {
                                                if (quoteLineItem.LineItemType.Contains("L"))
                                                {
                                                    punchWidth = die.WidthMinorAxis - .0010;
                                                    punchLength = die.LengthMajorAxis - .0010;
                                                }
                                                else
                                                {
                                                    punchWidth = die.WidthMinorAxis - .0015;
                                                    punchLength = die.LengthMajorAxis - .0015;
                                                }
                                            }
                                            if (_nat01Context.MachineList.Any(m => m.MachineNo == quoteLineItem.MachineNo))
                                            {
                                                MachineList machine = _nat01Context.MachineList.First(m => m.MachineNo == quoteLineItem.MachineNo);
                                                // Is B Machine
                                                if ((machine.UpperSize ?? machine.LowerSize) != @"3/4 x 5-3/4" && (machine.MachineTypePrCode.Trim() == "B" || machine.MachineTypePrCode.Trim() == "BB" || machine.MachineTypePrCode.Trim() == "BBS" ||
                                                                   ((machine.MachineTypePrCode.Trim() == "ZZZ" || machine.MachineTypePrCode.Trim() == "DRY") && (machine.UpperSize ?? machine.LowerSize) == @"1 x 5-3/4"))
                                                                    )
                                                {
                                                    if (die.LengthMajorAxis == .75 && !(quote.UserAcctNo == "1023804" && quote.UserLocNo == "02"))
                                                    {
                                                        punchWidth -= .0005;
                                                        punchLength -= .0005;
                                                    }
                                                }
                                                // Is D Machine
                                                if (machine.MachineTypePrCode.Trim() == "D" ||
                                                    ((machine.MachineTypePrCode.Trim() == "ZZZ" || machine.MachineTypePrCode.Trim() == "DRY") && (machine.UpperSize ?? machine.LowerSize) == @"1-1/4 x 5-3/4") ||
                                                    machine.MachineNo == 1015)
                                                {
                                                    if (die.LengthMajorAxis == 1.0 && !(quote.UserAcctNo == "1023804" && quote.UserLocNo == "02"))
                                                    {
                                                        punchWidth -= .0005;
                                                        punchLength -= .0005;
                                                    }
                                                }
                                            }

                                        }

                                        double? dieWidth = null;
                                        double? dieLength = null;
                                        // Special bore size
                                        if (dieQuoteLineItem.OptionNumbers.Contains("425"))
                                        {
                                            dieWidth = _nat01Context.QuoteOptionValueBDoubleNum.First(qov => qov.QuoteNo == Convert.ToInt32(quoteNo) && qov.RevNo == Convert.ToInt16(quoteRevNo) && !string.IsNullOrEmpty(qov.QuoteDetailType) && qov.QuoteDetailType.Trim() == dieQuoteLineItem.LineItemType && qov.OptionCode == "425").Number1;
                                            dieLength = _nat01Context.QuoteOptionValueBDoubleNum.First(qov => qov.QuoteNo == Convert.ToInt32(quoteNo) && qov.RevNo == Convert.ToInt16(quoteRevNo) && !string.IsNullOrEmpty(qov.QuoteDetailType) && qov.QuoteDetailType.Trim() == dieQuoteLineItem.LineItemType && qov.OptionCode == "425").Number2;
                                        }
                                        else
                                        {
                                            dieWidth = die.WidthMinorAxis;
                                            dieLength = die.LengthMajorAxis;
                                            if (_nat01Context.MachineList.Any(m => m.MachineNo == quoteLineItem.MachineNo))
                                            {
                                                MachineList machine = _nat01Context.MachineList.First(m => m.MachineNo == quoteLineItem.MachineNo);
                                                // Is B Machine
                                                if ((machine.UpperSize ?? machine.LowerSize) != @"3/4 x 5-3/4" && (machine.MachineTypePrCode.Trim() == "B" || machine.MachineTypePrCode.Trim() == "BB" || machine.MachineTypePrCode.Trim() == "BBS" ||
                                                                   ((machine.MachineTypePrCode.Trim() == "ZZZ" || machine.MachineTypePrCode.Trim() == "DRY") && (machine.UpperSize ?? machine.LowerSize) == @"1 x 5-3/4"))
                                                                    )
                                                {
                                                    if (die.LengthMajorAxis == .75 && !(quote.UserAcctNo == "1023804" && quote.UserLocNo == "02"))
                                                    {
                                                        dieWidth -= .0005;
                                                        dieLength -= .0005;
                                                    }
                                                }
                                                // Is D Machine
                                                if (machine.MachineTypePrCode.Trim() == "D" ||
                                                    ((machine.MachineTypePrCode.Trim() == "ZZZ" || machine.MachineTypePrCode.Trim() == "DRY") && (machine.UpperSize ?? machine.LowerSize) == @"1-1/4 x 5-3/4") ||
                                                    machine.MachineNo == 1015)
                                                {
                                                    if (die.LengthMajorAxis == 1.0 && !(quote.UserAcctNo == "1023804" && quote.UserLocNo == "02"))
                                                    {
                                                        dieWidth -= .0005;
                                                        dieLength -= .0005;
                                                    }
                                                }
                                            }
                                        }

                                        if ((punchWidth ?? 0) > (dieWidth ?? ((punchWidth ?? 0)+1)) || ((punchLength ?? -1) > (dieLength ?? (punchLength ?? -1) + 1)))
                                        {
                                            errors.Add("Tip sizes are larger than die bore.");
                                        }
                                        if (punchWidth + (quoteLineItem.LineItemType.Contains("L") ? .0035 : .0045) < dieWidth || (dieLength == null || dieLength == 0 ? false : punchLength + (quoteLineItem.LineItemType.Contains("L") ? .0035 : .0045) < dieLength))
                                        {
                                            errors.Add("Tip sizes seem too small compared to the die bore.");
                                        }
                                    }
                                }
                            }

                            // Lower Punch
                            if (quoteLineItem.LineItemType == "L")
                            {
                                // Is Bisected
                                if (!ContainsAny(hob.BisectCode, new List<string> { "000", "010", "020", "030" }, StringComparison.CurrentCulture))
                                {
                                    // Isn't Keyed
                                    if (!ContainsAny(string.Join(string.Empty, quoteLineItem.OptionNumbers), new List<string> { "130", "131", "132", "133", "139", "140", "141", "144" }, StringComparison.CurrentCulture))
                                    {
                                        errors.Add("'L' is bisected. Please add key at appropriate take-off angle.");
                                    }
                                    // Is Keyed
                                    else
                                    {
                                        // Key Angle is 0°
                                        if (quoteLineItem.Options.ContainsKey(156) || (quoteLineItem.Options.ContainsKey(155) && quoteLineItem.Options[155].Contains(" 0° ")))
                                        {
                                            errors.Add("'L' is bisected. Please check that the key is oriented for proper take-off.");
                                        }
                                    }
                                }

                                // Is Reduced Tip Width || Is Strengthened Tip || Is Solid MultiTip || Carbide Tipped
                                if ((quoteLineItem.Options.ContainsKey(204) && _nat01Context.QuoteOptionValueASingleNum.Any(qo => qo.QuoteNo == Convert.ToInt32(quoteNo) && qo.RevNo == Convert.ToInt16(quoteRevNo) && qo.QuoteDetailType.Trim() == quoteLineItem.LineItemType && qo.OptionCode == "204" && qo.Number1 < .1875)) ||
                                quoteLineItem.OptionNumbers.Contains("222") ||
                                quoteLineItem.TipQTY > 1 ||
                                quoteLineItem.OptionNumbers.Contains("240"))
                                {
                                    // Machine Number Exists
                                    if (_nat01Context.MachineList.Any(m => quoteLineItem.MachineNo != null && quoteLineItem.MachineNo > 0 && m.MachineNo == quoteLineItem.MachineNo))
                                    {
                                        MachineList machine = _nat01Context.MachineList.First(m => quoteLineItem.MachineNo != null && quoteLineItem.MachineNo > 0 && m.MachineNo == quoteLineItem.MachineNo);
                                        // Is B Machine
                                        if ((machine.UpperSize ?? machine.LowerSize) != @"3/4 x 5-3/4" && (machine.MachineTypePrCode.Trim() == "B" || machine.MachineTypePrCode.Trim() == "BB" || machine.MachineTypePrCode.Trim() == "BBS" ||
                                           ((machine.MachineTypePrCode.Trim() == "ZZZ" || machine.MachineTypePrCode.Trim() == "DRY") && (machine.UpperSize ?? machine.LowerSize) == @"1 x 5-3/4"))
                                            )
                                        {
                                            // Has Special Barrel Diameter
                                            if (quoteLineItem.OptionNumbers.Contains("120"))
                                            {
                                                // Barrel Diameter less than .7475
                                                if (_nat01Context.QuoteOptionValueASingleNum.Any(qo => qo.QuoteNo == Convert.ToInt32(quoteNo) && qo.RevNo == Convert.ToInt16(quoteRevNo) && qo.QuoteDetailType.Trim() == quoteLineItem.LineItemType && qo.OptionCode == "120" && qo.Number1 < .7475))
                                                {
                                                    errors.Add("'L' has reduced tip width, strengthened lower tip, multi-tipped, or carbide tipped. Check to see if increasing the barrel diameter is possible to improve stability.");
                                                }
                                            }
                                            // Reduced tip width
                                            else if(quoteLineItem.Options.ContainsKey(204) && _nat01Context.QuoteOptionValueASingleNum.Any(qo => qo.QuoteNo == Convert.ToInt32(quoteNo) && qo.RevNo == Convert.ToInt16(quoteRevNo) && qo.QuoteDetailType.Trim() == quoteLineItem.LineItemType && qo.OptionCode == "204" && qo.Number1 < .1875))
                                            {
                                                bool eu = false;
                                                if (quoteLineItem.OptionNumbers.Contains("116") && machine.Description.ToUpper().Contains("SYNTHESIS"))
                                                {
                                                    eu = true;
                                                }
                                                else
                                                {
                                                    foreach (QuoteLineItem qli in quoteLineItems)
                                                    {
                                                        if (ContainsAny(string.Join(string.Empty, quoteLineItem.OptionNumbers), new List<string> { "333", "003", "004", "005", "006", "007", "009" }, StringComparison.CurrentCulture))
                                                        {
                                                            eu = true;
                                                        }
                                                    }
                                                    // TSM Type
                                                    if (eu == false)
                                                    {
                                                        errors.Add("'L' has reduced tip width. Check to see if increasing the barrel diameter is possible to improve stability.");
                                                    }
                                                }
                                            }
                                        }
                                        // Is D Machine
                                        if (machine.MachineTypePrCode.Trim() == "D" ||
                                            ((machine.MachineTypePrCode.Trim() == "ZZZ" || machine.MachineTypePrCode.Trim() == "DRY") && (machine.UpperSize ?? machine.LowerSize) == @"1-1/4 x 5-3/4") ||
                                            machine.MachineNo == 1015)
                                        {
                                            // Has Special Barrel Diameter
                                            if (quoteLineItem.OptionNumbers.Contains("120"))
                                            {
                                                // Barrel Diameter less than .9975
                                                if (_nat01Context.QuoteOptionValueASingleNum.Any(qo => qo.QuoteNo == Convert.ToInt32(quoteNo) && qo.RevNo == Convert.ToInt16(quoteRevNo) && qo.QuoteDetailType.Trim() == quoteLineItem.LineItemType && qo.OptionCode == "120" && qo.Number1 < .9975))
                                                {
                                                    errors.Add("'L' has reduced tip width, strengthened lower tip, multi-tipped, or carbide tipped. Check to see if increasing the barrel diameter is possible to improve stability.");
                                                }
                                            }
                                            // Reduced tip width
                                            else if (quoteLineItem.Options.ContainsKey(204) && _nat01Context.QuoteOptionValueASingleNum.Any(qo => qo.QuoteNo == Convert.ToInt32(quoteNo) && qo.RevNo == Convert.ToInt16(quoteRevNo) && qo.QuoteDetailType.Trim() == quoteLineItem.LineItemType && qo.OptionCode == "204" && qo.Number1 < .1875))
                                            {
                                                bool eu = false;
                                                if (quoteLineItem.OptionNumbers.Contains("116") && machine.Description.ToUpper().Contains("SYNTHESIS"))
                                                {
                                                    eu = true;
                                                }
                                                else
                                                {
                                                    foreach (QuoteLineItem qli in quoteLineItems)
                                                    {
                                                        if (ContainsAny(string.Join(string.Empty, quoteLineItem.OptionNumbers), new List<string> { "333", "003", "004", "005", "006", "007", "009" }, StringComparison.CurrentCulture))
                                                        {
                                                            eu = true;
                                                        }
                                                    }
                                                    // TSM Type
                                                    if (eu == false)
                                                    {
                                                        errors.Add("'L' has reduced tip width. Check to see if increasing the barrel diameter is possible to improve stability.");
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            errors.Add("'" + quoteLineItem.LineItemType + "' does not have a valid Hob Number. Check your Hob Number, Tip QTY, and Bore Circle.");
                        }
                    }

                    // Alignment Tool
                    if (quoteLineItem.LineItemType == "A")
                    {
                        // Need Appropriate interchangeability options
                        if (quoteLineItems.Any(qli => (qli.LineItemType == "U" || qli.LineItemType == "UH") && !qli.OptionNumbers.Contains("160")))
                        {
                            errors.Add("'" + quoteLineItems.First(qli => (qli.LineItemType == "U" || qli.LineItemType == "UH") && !qli.OptionNumbers.Contains("160")).LineItemType + "' needs (422) SPECIAL BORE CONCENTRICITY for 100% interchangeability");
                        }
                    }

                }

            }

            //If(IfError(ShortRejectTipGenerationError = 36, 0), "[color red]Check number of bores in segment on order/quote.[/color]", "Check number of bores in segment on order/quote.") & NewLine() &

            //If(IfError(ShortRejectCapGenerationError = 37, 0), "[color red]Key angle on order does not match customer machines list.[/color]", "Key angle on order does not match customer machines list.") & NewLine() &

            //If(IfError(UpperHeadGenerationError = 42, 0), "[color red]Machine Numbers do not match.[/color]", "Machine Numbers do not match.") & NewLine() &

            //If(IfError(LowerHolderGenerationError = 43, 0), "[color red]Upper needs key[/color]", "Upper needs key.") & NewLine() &

            //If(IfError(LowerHeadGenerationError = 44, 0), "[color red]Key needs segment interchangeability options[/color]", "Key needs segment interchangeability options") & NewLine() &

            //If(IfError(ShortRejectHolderGenerationError = 45, 0), "[color red]Cup depths differ[/color]", "Cup depths differ") & NewLine() &

            //If(IfError(ShortRejectHeadGenerationError = 46, 0), "[color red]Land values differ.[/color]", "Land values differ.") & NewLine() &

            //If(IfError(GrooveNumberError = 52, 0), "[color red]Carbide is not assigned[/color]", "Carbide is not assigned") & NewLine() &

            //If(IfError(AlignmentInchError = 58, 0), "[color red]Key Angles are not consistent.[/color]", "Key Angles are not consistent.") & NewLine() &

            //If(IfError(AlignmentMetricError = 59, 0), "[color red]Can only micro-mirror finish the barrel of B and D tooling.[/color]", "Can only micro-mirror finish the barrel of B and D tooling.") & NewLine() &

            //If(IfError(AlignmentInch2Error = 60, 0), "[color red]Overall Length could be incorrect.[/color]", "Overall Length could be incorrect.") & NewLine() &

            //If(IfError(AlignmentMetric2Error = 61, 0), "[color red]Reject needs correct overall length option.[/color]", "Reject needs correct overall length option.") & NewLine() &

            //If(IfError(DieInchError = 62, 0), "[color red]Need appropriate undercut die options.[/color]", "Need appropriate undercut die options.") & NewLine() &

            //If(IfError(RejectInchError = 63, 0), "[color red]No woodruff keys in PM steel.[/color]", "No woodruff keys in PM steel.") & NewLine() &

            //If(IfError(LowerInch2Error = 64, 0), "[color red]Tip relief description is incorrect.[/color]", "Tip relief description is incorrect.") & NewLine() &

            //If(IfError(DieMaterialError = 65, 0), "[color red]Please specify die insert material.[/color]", "Please specify die insert material.") & NewLine() &

            //If(IfError(UpperPunchMaterialError = 66, 0), "[color red]Check tolerance, cup is flat face.[/color]", "Check tolerance, cup is flat face.") & NewLine() &

            //If(IfError(LowerInchError = 67, 0), "[color red]Hold low barrel tolerance is not required for this machine.[/color]", "Hold low barrel tolerance is not required for this machine.") & NewLine() &

            //If(IfError(LowerMetric2Error = 68, 0), "[color red]Tip length for oil seals may not be required for this machine.[/color]", "Tip length for oil seals may not be required for this machine.") & NewLine() &

            //If(IfError(DieMetricError = 69, 0), "[color red]Check to see if A2 Steel (Die) is required for Exotic/Semi-Exotic Shape.[/color]", "Check to see if A2 Steel (Die) is required for Exotic/Semi-Exotic Shape.") & NewLine() &

            //If(IfError(DieInch2Error = 70, 0), "[color red]Carbide lined segments should be A2 Steel.[/color]", "Carbide lined segments should be A2 Steel.") & NewLine() &

            //If(IfError(DieMetric2Error = 71, 0), "[color red]Exceeded max size for BBS Die.[/color]", "Exceeded max size for BBS Die.") & NewLine() &

            //If((DWVariableErrorMessageUpper & DWVariableErrorMessageLower & DWVariableErrorMessageLongReject & DWVariableErrorMessageShortReject) <> "", "[color red]Key angle not in Customer machine list. Please verify and enter key angle in Magic.[/color]", "Key angle not in Customer machine list. Please verify and enter key angle in Magic.") & NewLine() &

            //If(ErrorLabel1Error, "[color red]Think about whether strengthened tips/undercut dies are necessary.[/color]", "Think about whether strengthened tips/undercut dies are necessary.") & NewLine() &

            //If(DWVariableNewHobError, "[color red]New hob line item MAY be required and missing for one of the hobs on this quote/order.[/color]", "New hob line item MAY be required and missing for one of the hobs on this quote/order.") & NewLine() &

            //If(IfError(UpperCapMaterialError = 72, 0), "[color red]Grooves on Stokes machines not recommended.[/color]", "Grooves on Stokes machines not recommended.") & NewLine() &

            //If(IfError(FolderLocationError = 73, 0), "[color red]Order/quote does not contain any detail type ID's that can be run through this specification.[/color]", "Order/quote does not contain any detail type ID's that can be run through this specification.") & NewLine() &

            //If(IfError(LongRejectCapMaterialError = 74, 0), "[color red]Please choose the other H13 from the steel list.[/color]", "Please choose the other H13 from the steel list.") & NewLine() &

            //If(IfError(UpperTipMaterialError = 75, 0), "[color red]Special Upper Key requires line item.[/color]", "Special Upper Key requires line item.") & NewLine() &

            //If(IfError(LowerTipMaterialError = 76, 0), "[color red]Special Lower Key requires line item.[/color]", "Special Lower Key requires line item.") & NewLine() &

            //If(IfError(ShortRejectTipMaterialError = 77, 0), "[color red]Special ShortReject Key requires line item.[/color]", "Special ShortReject Key requires line item.") & NewLine() &

            //If(IfError(LongRejectTipMaterialError = 78, 0), "[color red]Special LongReject Key requires line item.[/color]", "Special LongReject Key requires line item.") & NewLine() &

            //If(IfError(Label1Error = 79, 0), "[color red]Too large to hob.[/color]", "Too large to hob.") & NewLine() &

            //If(IfError(UpperNotFlushWLError = 80, 0), "[color red]Upper requires Tip Concentricity or Hold Low Tip Size Tolerance.[/color]", "Upper requires Tip Concentricity or Hold Low Tip Size Tolerance.") & NewLine() &

            //If(IfError(LowerNotFlushWLError = 81, 0), "[color red]Lower requires Tip Concentricity or Hold Low Tip Size Tolerance.[/color]", "Lower requires Tip Concentricity or Hold Low Tip Size Tolerance.") & NewLine() &

            //If(IfError(ShortRejectNotFlushWLError = 82, 0), "[color red]ShortReject requires Tip Concentricity or Hold Low Tip Size Tolerance.[/color]", "ShortReject requires Tip Concentricity or Hold Low Tip Size Tolerance.") & NewLine() &

            //If(IfError(LongRejectNotFlushWLError = 83, 0), "[color red]LongReject requires Tip Concentricity or Hold Low Tip Size Tolerance.[/color]", "LongReject requires Tip Concentricity or Hold Low Tip Size Tolerance.")

            quote.Dispose();
            _nat01Context.Dispose();
            _driveworksContext.Dispose();
            return errors;
        }
    }
}
