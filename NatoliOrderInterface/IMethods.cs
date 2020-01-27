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
using System.Diagnostics;
using System.Threading.Tasks;

namespace NatoliOrderInterface
{
    
    public interface IMethods
    {
        /// <summary>
        /// Writes to error log file at @"\\engserver\workstations\NatoliOrderInterfaceErrorLog\Error_Log.txt". 'errorLoc' should describe the location in the code. 'errorMessage' should be the Exception.Message. 'user' can be null if it has not been given a value yet.
        /// </summary>
        /// <param name="errorLoc"></param>
        /// <param name="errorMessage"></param>
        /// <param name="user"></param>
        public static void WriteToErrorLog(string errorLoc, string errorMessage, User user)
        {
            try
            {
                string path = @"\\engserver\workstations\NatoliOrderInterfaceErrorLog\Error_Log.txt";
                System.IO.StreamReader sr = new System.IO.StreamReader(path);
                string existing = sr.ReadToEnd();
                existing = existing.TrimEnd();
                sr.Close();
                System.IO.StreamWriter sw = new System.IO.StreamWriter(path, false);
                sw.Write(
                    "Version: " + user == null ? "No Information" : user.PackageVersion + "\r\n\t" +
                    "DateTime: " + DateTime.Now + "\r\n\t" +
                    "User: " + (user == null ? Environment.UserDomainName : user.GetUserName()) + "\r\n\t" +
                    "Location: " + errorLoc + "\r\n\t" +
                    "ErrorMessage: " + (errorMessage == null ? "" : errorMessage) + "\r\n" + "\r\n" + new string('+', 50) + "\r\n" + "\r\n" +
                    existing);
                sw.Flush();
                sw.Close();
            }
            catch
            {
                MessageBox.Show("Error in the Error Handling of Errors");
            }
        }

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
        /// Checks for 'processName' in processes and brings to front.
        /// </summary>
        /// <param name="processName"></param>
        public static void BringProcessToFrontByName(string processName)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            try
            {
                // Get process Name
                
                process = System.Diagnostics.Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(processName))[0];
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
        /// Brings 'process' to front by process ID.
        /// </summary>
        /// <param name="process"></param>
        public static void BringProcessToFront(Process process)
        {
            IntPtr handle = NativeMethods.FindWindow(null, process.MainWindowTitle);
            if (handle == IntPtr.Zero)
            {
                return;
            }
            NativeMethods.SetForegroundWindow(handle);
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
        public static List<string> QuoteErrors(string quoteNo, string quoteRevNo, User user)
        {
            List<string> errors = new List<string>();
            try
            {
                // When querying option values be sure to check if it exists in QuoteLineItem.OptionNumber before directly querying the optionvalue table to avoid orphan data.
                using var _nat01Context = new NAT01Context();
                using var _nat02Context = new NAT02Context();
                using var _driveworksContext = new DriveWorksContext();
                Quote quote = new Quote(Convert.ToInt32(quoteNo), Convert.ToInt16(quoteRevNo));
                List<QuoteDetails> quoteDetails = quote.Nat01Context.QuoteDetails.Where(l => (int)l.QuoteNo == Convert.ToInt32(quoteNo) && l.Revision == Convert.ToInt16(quoteRevNo)).OrderBy(q => q.LineNumber).ToList();
                List<QuoteLineItem> quoteLineItems = new List<QuoteLineItem>();
                foreach (QuoteDetails line in quoteDetails)
                {
                    quoteLineItems.Add(new QuoteLineItem(quote, line.LineNumber));
                }

                if (quoteLineItems.Count > 0)
                {
                    try
                    {
                        // Has a machine on the order
                        if (quoteLineItems.Any(ql => ql.MachineNo != null && ql.MachineNo > 0))
                        {
                            short _machineNo = quoteLineItems.First(ql => ql.MachineNo != null && ql.MachineNo > 0).MachineNo ?? -1;
                            // Machine Not Set Up For End User
                            if (!_nat01Context.CustomerMachines.Any(m => !string.IsNullOrEmpty(m.CustomerNo) && !string.IsNullOrEmpty(m.CustAddressCode) &&
                            m.CustomerNo.Trim() == quote.UserAcctNo &&
                            m.CustAddressCode.Trim() == quote.UserLocNo &&
                            (m.MachineNo ?? -2) == _machineNo))
                            {
                                errors.Add("Machine " + quoteLineItems.First(ql => ql.MachineNo != null && ql.MachineNo > 0).MachineNo + " not setup for " + quote.UserAcctNo + " - " + quote.UserLocNo + ".");
                            }
                            // Tablet is Too Large for press
                            if (_nat01Context.MachineList.Any(m => m.MachineNo == _machineNo))
                            {
                                MachineList machine = _nat01Context.MachineList.First(m => m.MachineNo == _machineNo);
                                // Is B Machine
                                if ((machine.UpperSize.Trim() ?? machine.LowerSize.Trim()) != @"3/4 x 5-3/4" && (machine.MachineTypePrCode.Trim() == "B" || machine.MachineTypePrCode.Trim() == "BB" || machine.MachineTypePrCode.Trim() == "BBS" ||
                                                   ((machine.MachineTypePrCode.Trim() == "ZZZ" || machine.MachineTypePrCode.Trim() == "DRY") && (machine.UpperSize.Trim() ?? machine.LowerSize.Trim()) == @"1 x 5-3/4"))
                                                    )
                                {
                                    // Has A Hob W/ a Die ID of width or length > .75
                                    if (quoteLineItems.Any(qli => qli.LineItemType != "D" && qli.LineItemType != "DS" && qli.LineItemType != "DA" && qli.LineItemType != "DC" && qli.LineItemType != "DI" && qli.LineItemType != "DP" &&
                                     (!string.IsNullOrWhiteSpace(qli.HobNoShapeID) && _nat01Context.HobList.Any(h => h.HobNo == qli.HobNoShapeID && h.TipQty == (qli.TipQTY ?? 1) && h.BoreCircle == (qli.BoreCircle ?? 0)) && _nat01Context.DieList.Any(d => !string.IsNullOrEmpty(d.DieId) && d.DieId.Trim() == _nat01Context.HobList.First(h => h.HobNo == qli.HobNoShapeID && h.TipQty == (qli.TipQTY ?? 1) && h.BoreCircle == (qli.BoreCircle ?? 0)).DieId.Trim() && (d.LengthMajorAxis > .75 || d.WidthMinorAxis > .75)))))
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
                                    ((machine.MachineTypePrCode.Trim() == "ZZZ" || machine.MachineTypePrCode.Trim() == "DRY") && (machine.UpperSize.Trim() ?? machine.LowerSize.Trim()) == @"1-1/4 x 5-3/4") ||
                                    machine.MachineNo == 1015)
                                {
                                    // Has A Hob W/ a Die ID of width or length > 1.0
                                    if (quoteLineItems.Any(qli => qli.LineItemType != "D" && qli.LineItemType != "DS" && qli.LineItemType != "DA" && qli.LineItemType != "DC" && qli.LineItemType != "DI" && qli.LineItemType != "DP" &&
                                     (!string.IsNullOrWhiteSpace(qli.HobNoShapeID) && _nat01Context.HobList.Any(h => h.HobNo == qli.HobNoShapeID && h.TipQty == (qli.TipQTY ?? 1) && h.BoreCircle == (qli.BoreCircle ?? 0)) && _nat01Context.DieList.Any(d => !string.IsNullOrEmpty(d.DieId) && d.DieId.Trim() == _nat01Context.HobList.First(h => h.HobNo == qli.HobNoShapeID && h.TipQty == (qli.TipQTY ?? 1) && h.BoreCircle == (qli.BoreCircle ?? 0)).DieId.Trim() && (d.LengthMajorAxis > 1.0 || d.WidthMinorAxis > 1.0)))))
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

                        // Carbide not Assigned
                        if (quoteLineItems.Any(qli => qli.OptionNumbers.Contains("491")) && !_nat02Context.PartAllocation.Any(pa => pa.QuoteNumber == quoteNo && pa.QuoteRevNo == Convert.ToInt32(quoteRevNo)))
                        {
                            errors.Add("Carbide has not been assigned.");
                        }

                        if (quoteLineItems.Count > 1)
                        {
                            // Has shortened lower tip || shallow fill cam || undercut die
                            if (quoteLineItems.Any(qli => qli.OptionNumbers.Contains("222") || qli.OptionNumbers.Contains("226") || qli.OptionNumbers.Contains("460")))
                            {
                                // Has Die
                                if (quoteLineItems.Any(qli => qli.LineItemType == "D" || qli.LineItemType == "DS"))
                                {
                                    // No undercut
                                    if (!quoteLineItems.Any(qli => (qli.LineItemType == "D" || qli.LineItemType == "DS") && qli.OptionNumbers.Contains("460")))
                                    {
                                        errors.Add("Die needs undercut option (460).");
                                    }
                                }
                                // Has Lower Assembly
                                if (quoteLineItems.Any(qli => qli.LineItemType == "LA"))
                                {
                                    // Has Lower Tip
                                    if (quoteLineItems.Any(qli => qli.LineItemType == "LT"))
                                    {
                                        if (!(quoteLineItems.Any(qli => (qli.LineItemType == "LT" || qli.LineItemType == "LA") && qli.OptionNumbers.Contains("222")) && quoteLineItems.Any(qli => qli.LineItemType == "LA" && qli.OptionNumbers.Contains("226"))))
                                        {
                                            errors.Add("Lower Tip or Lower Assembly needs Shortened Lower Tip (222) or Lower Assembly needs Shallow Fill Cam (226).");
                                        }
                                    }
                                    else
                                    {
                                        if (!quoteLineItems.Any(qli => qli.LineItemType == "LA" && qli.OptionNumbers.Contains("222") && qli.OptionNumbers.Contains("226")))
                                        {
                                            errors.Add("Lower Assembly needs Shortened Lower Tip (222) and/or Shallow Fill Cam (226).");
                                        }
                                    }
                                }
                                else if (quoteLineItems.Any(qli => qli.LineItemType == "L"))
                                {
                                    if (!quoteLineItems.Any(qli => qli.LineItemType == "L" && qli.OptionNumbers.Contains("222") && qli.OptionNumbers.Contains("226")))
                                    {
                                        errors.Add("Lower needs Shortened Lower Tip (222) and/or Shallow Fill Cam (226).");
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Add("Could not finish checking for errors. Please let someone know that it failed.");
                        WriteToErrorLog("QuoteErrors => if (quoteLineItems.Count > 0)", ex.Message, user);
                    }

                    QuoteOptionValueCTolerance workingLengthTolerance = null;
                    bool varyingWLTolerances = false;
                    string machineDescription = null;
                    string shapeDescription = "";
                    short? machineNumber = null;
                    float? cupDepth = null;
                    float? land = null;
                    short? quoteKeyAngle = null;
                    string dieNumber = null;
                    foreach (QuoteLineItem quoteLineItem in quoteLineItems)
                    {
                        // Check valid line item
                        if (string.IsNullOrEmpty(quoteLineItem.LineItemType) || string.IsNullOrWhiteSpace(quoteLineItem.LineItemType) || !_nat01Context.OedetailType.Any(o => !string.IsNullOrWhiteSpace(o.TypeId) && o.TypeId.Trim() == quoteLineItem.LineItemType))
                        {
                            errors.Add("Line Item Number '" + quoteLineItem.LineItemNumber + "' does not have a valid Line Item Type.");
                        }
                        else
                        {
                            // OL
                            {
                                if (quoteLineItem.OptionNumbers.Contains("330") && quoteLineItem.optionValuesA.Any(ov => ov.OptionCode == "330" && ov.Number1 == 5.2598))
                                {
                                    errors.Add("'" + quoteLineItem.LineItemType + "' has (330) SPECIAL OVERALL LENGTH 5.2598\". This option should be removed and (333) 133.6mm (5.2598\") OVERALL LENGTH should be added.");
                                }
                                if (quoteLineItem.OptionNumbers.Contains("330") && quoteLineItem.optionValuesA.Any(ov => ov.OptionCode == "330" && ov.Number1 == 5.2500))
                                {
                                    errors.Add("'" + quoteLineItem.LineItemType + "' has (330) SPECIAL OVERALL LENGTH 5.2500\". This option should be removed and (332) 133.35mm (5.2500\") OVERALL LENGTH should be added.");
                                }
                                int olOptionsCount = 0;
                                foreach (string optionNumber in quoteLineItem.OptionNumbers)
                                {
                                    if (optionNumber == "330" || optionNumber == "332" || optionNumber == "333" || optionNumber == "350" || optionNumber == "354")
                                    {
                                        olOptionsCount++;
                                    }
                                }
                                if (olOptionsCount > 1)
                                {
                                    errors.Add("'" + quoteLineItem.LineItemType + "' has at least two overall length options added. Please remove until one is left.");
                                }
                                if (olOptionsCount > 0 && quoteLineItem.OptionNumbers.Contains("328"))
                                {
                                    errors.Add("'" + quoteLineItem.LineItemType + "' has special overall length and special working length. Please remove one.");
                                }
                            }


                            // Comparisons
                            if (quoteLineItems.Count > 1)
                            {
                                // Shape Description
                                if (!string.IsNullOrWhiteSpace(quoteLineItem.HobNoShapeID) && (_nat01Context.HobList.Any(h => h.HobNo == quoteLineItem.HobNoShapeID && h.TipQty == (quoteLineItem.TipQTY ?? 1) && h.BoreCircle == (quoteLineItem.BoreCircle ?? 0)) || _nat01Context.DieList.Any(d => d.DieId == quoteLineItem.HobNoShapeID)))
                                {
                                    if (!string.IsNullOrEmpty(shapeDescription) && (string.IsNullOrEmpty(quoteLineItem.Desc2) || string.IsNullOrWhiteSpace(quoteLineItem.Desc2) || quoteLineItem.Desc2.Trim() != shapeDescription.Trim()))
                                    {
                                        errors.Add("Shape Descriptions '" + shapeDescription + "' and '" + (string.IsNullOrEmpty(quoteLineItem.Desc2) ? "NULL" : quoteLineItem.Desc2.Trim()) + "' do not match.");
                                    }
                                    shapeDescription = quoteLineItem.Desc2 == null ? "" : quoteLineItem.Desc2.Trim();
                                    // Hob
                                    if ((quoteLineItem.LineItemType == "U" || quoteLineItem.LineItemType == "UT" ||
                                        quoteLineItem.LineItemType == "L" || quoteLineItem.LineItemType == "LT" ||
                                        quoteLineItem.LineItemType == "R" || quoteLineItem.LineItemType == "RT" ||
                                        quoteLineItem.LineItemType == "H") && quoteLineItem.LineItemType != "MS" && !string.IsNullOrWhiteSpace(quoteLineItem.HobNoShapeID) && _nat01Context.HobList.Any(h => h.HobNo == quoteLineItem.HobNoShapeID && h.TipQty == (quoteLineItem.TipQTY ?? 1) && h.BoreCircle == (quoteLineItem.BoreCircle ?? 0)))
                                    {
                                        HobList hob = _nat01Context.HobList.First(h => h.HobNo == quoteLineItem.HobNoShapeID && h.TipQty == (quoteLineItem.TipQTY ?? 1) && h.BoreCircle == (quoteLineItem.BoreCircle ?? 0));
                                        // Cup Depth
                                        if (cupDepth != null && hob.CupDepth != cupDepth)
                                        {
                                            errors.Add("'" + quoteLineItem.LineItemType + "' Cup Depth does not match another line item's.");
                                        }
                                        cupDepth = hob.CupDepth;
                                        // Land
                                        if (land != null && hob.Land != land)
                                        {
                                            errors.Add("'" + quoteLineItem.LineItemType + "' Land does not match another line item's.");
                                        }
                                        land = hob.Land;
                                        if (dieNumber != null)
                                        {
                                            // Dies do not match
                                            if (dieNumber != hob.DieId)
                                            {
                                                errors.Add("'" + quoteLineItem.LineItemType + "' Die Number does not match another line item's.");
                                            }
                                        }

                                        dieNumber = hob.DieId ?? "None";
                                    }
                                    // Die
                                    else
                                    {
                                        if (!string.IsNullOrWhiteSpace(quoteLineItem.HobNoShapeID) && _nat01Context.DieList.Any(d => !string.IsNullOrWhiteSpace(d.DieId) && d.DieId == quoteLineItem.HobNoShapeID))
                                        {
                                            DieList die = _nat01Context.DieList.First(d => !string.IsNullOrWhiteSpace(d.DieId) && d.DieId == quoteLineItem.HobNoShapeID);
                                            if (dieNumber != null)
                                            {
                                                // Dies do not match
                                                if (dieNumber != die.DieId)
                                                {
                                                    errors.Add("'" + quoteLineItem.LineItemType + "' Die Number does not match another line item's.");
                                                }
                                            }
                                            dieNumber = die.DieId;
                                        }
                                    }

                                }



                            }

                            // Machine #'s and Descriptions
                            if (quoteLineItem.MachineNo != null && quoteLineItem.MachineNo > 0)
                            {

                                if (machineNumber == null)
                                {
                                    machineNumber = quoteLineItem.MachineNo;
                                }
                                else
                                {
                                    // Machine Numbers are different
                                    if (machineNumber != quoteLineItem.MachineNo)
                                    {
                                        errors.Add("Machine numbers are different at least two items.");
                                    }
                                    machineNumber = quoteLineItem.MachineNo;
                                }
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

                            // Not Tip or Punch or Hob or D or DS OR M OR MS
                            if (quoteLineItem.LineItemType == "UHD" || quoteLineItem.LineItemType == "UA" || quoteLineItem.LineItemType == "UC" || quoteLineItem.LineItemType == "UH" ||
                                 quoteLineItem.LineItemType == "LHD" || quoteLineItem.LineItemType == "LA" || quoteLineItem.LineItemType == "LC" || quoteLineItem.LineItemType == "LH" ||
                                 quoteLineItem.LineItemType == "RHD" || quoteLineItem.LineItemType == "RA" || quoteLineItem.LineItemType == "RC" || quoteLineItem.LineItemType == "RH")
                            {
                                // Has hob #
                                if (!string.IsNullOrWhiteSpace(quoteLineItem.HobNoShapeID) && !string.IsNullOrEmpty(quoteLineItem.HobNoShapeID))
                                {
                                    errors.Add("'" + quoteLineItem.LineItemType + "' has a hob number '" + quoteLineItem.HobNoShapeID + "' on its line.");
                                }
                            }

                            // Upper, Lower, Reject
                            if (quoteLineItem.LineItemType == "U" ||
                                quoteLineItem.LineItemType == "L" || quoteLineItem.LineItemType == "LCRP" ||
                                quoteLineItem.LineItemType == "R")
                            {
                                try
                                {

                                    // Upper
                                    if (quoteLineItem.LineItemType == "U" || quoteLineItem.LineItemType == "R")
                                    {
                                        // Machine Exists
                                        if (_nat01Context.MachineList.Any(m => quoteLineItem.MachineNo != null && quoteLineItem.MachineNo > 0 && m.MachineNo == quoteLineItem.MachineNo))
                                        {
                                            MachineList machine = _nat01Context.MachineList.First(m => quoteLineItem.MachineNo != null && quoteLineItem.MachineNo > 0 && m.MachineNo == quoteLineItem.MachineNo);
                                            // Is B or D Machine
                                            if (((machine.UpperSize.Trim() ?? machine.LowerSize.Trim()) != @"3/4 x 5-3/4" && (machine.MachineTypePrCode.Trim() == "B" || machine.MachineTypePrCode.Trim() == "BB" || machine.MachineTypePrCode.Trim() == "BBS" ||
                                                ((machine.MachineTypePrCode.Trim() == "ZZZ" || machine.MachineTypePrCode.Trim() == "DRY") && (machine.UpperSize.Trim() ?? machine.LowerSize.Trim()) == @"1 x 5-3/4")))
                                                ||
                                                (machine.MachineTypePrCode.Trim() == "D" ||
                                                    ((machine.MachineTypePrCode.Trim() == "ZZZ" || machine.MachineTypePrCode.Trim() == "DRY") && (machine.UpperSize.Trim() ?? machine.LowerSize.Trim()) == @"1-1/4 x 5-3/4") ||
                                                    machine.MachineNo == 1015))
                                            {
                                                // Is NOT Apotex
                                                if (!(quote.UserAcctNo == "1031250" || quote.UserAcctNo == "1001400"))
                                                {
                                                    // Machine requires tip length for oil seals
                                                    if (ContainsAny(machine.Description, new List<string> { "FETTE", "KILIAN", "KORSCH", "X-PRESS", "XS-PRESS" }, StringComparison.InvariantCultureIgnoreCase) || (machine.Description.Contains("GENESIS", StringComparison.InvariantCultureIgnoreCase) && machine.Description.Contains("STOKES", StringComparison.InvariantCultureIgnoreCase)) || machine.SpecialInfo.Contains("102"))
                                                    {
                                                        // Does not have option 102 or 103
                                                        if (!quoteLineItem.OptionNumbers.Contains("102") && !quoteLineItem.OptionNumbers.Contains("103"))
                                                        {
                                                            // Does not have grooves or reduced barrel
                                                            if (!ContainsAny(string.Join(",", quoteLineItem.OptionNumbers), new List<string> { "110", "111", "112", "115", "116", "117" }, StringComparison.CurrentCulture))
                                                            {
                                                                errors.Add("'" + quoteLineItem.LineItemType + "' may require tip length for oil seals.");
                                                            }
                                                        }
                                                    }
                                                }
                                                // Machine Does Not require Tip Length for oil seals
                                                if (!ContainsAny(machine.Description, new List<string> { "FETTE", "KILIAN", "KORSCH", "X-PRESS", "XS-PRESS" }, StringComparison.InvariantCultureIgnoreCase) && !(machine.Description.Contains("GENESIS", StringComparison.InvariantCultureIgnoreCase) && machine.Description.Contains("STOKES", StringComparison.InvariantCultureIgnoreCase)) && !machine.SpecialInfo.Contains("102"))
                                                {
                                                    if (quoteLineItem.OptionNumbers.Contains("102"))
                                                    {
                                                        errors.Add("'" + quoteLineItem.LineItemType + "' may NOT require tip length for oil seals.");
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    if (!varyingWLTolerances)
                                    {
                                        if (quoteLineItem.OptionNumbers.Contains("336"))
                                        {
                                            try
                                            {
                                                if (workingLengthTolerance == null)
                                                {
                                                    try
                                                    {
                                                        workingLengthTolerance = quoteLineItem.optionValuesC.First(qov => qov.OptionCode == "336");
                                                    }
                                                    catch
                                                    {
                                                        errors.Add("'" + quoteLineItem.LineItemType + "' has option (336) without a value.");
                                                    }
                                                }
                                                else if (workingLengthTolerance.TopValue != quoteLineItem.optionValuesC.First(qov => qov.OptionCode == "336").TopValue || workingLengthTolerance.BottomValue != quoteLineItem.optionValuesC.First(qov => qov.OptionCode == "336").BottomValue)
                                                {
                                                    errors.Add("Working Length Tolerances vary. Check to make sure they contain correct values.");
                                                }
                                            }
                                            catch
                                            {
                                                errors.Add("'" + quoteLineItem.LineItemType + "' has option (336) without a value.");
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
                                catch (Exception ex)
                                {
                                    errors.Add("Failed checking for errors on '" + quoteLineItem.LineItemType + "'. Please let someone know.");
                                    WriteToErrorLog("QuoteErrors => Upper, Lower, Reject", ex.Message, user);
                                }
                            }

                            // Die
                            if (quoteLineItem.LineItemType == "D")
                            {
                                try
                                {
                                    // No Die Groove
                                    if (!ContainsAny(string.Join(",", quoteLineItem.OptionNumbers), new List<string> { "430", "431", "432", "433", "434", "435", "436", "437", "438", "439" }, StringComparison.CurrentCulture))
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
                                    if (!string.IsNullOrWhiteSpace(quoteLineItem.HobNoShapeID) && _nat01Context.DieList.Any(d => !string.IsNullOrEmpty(d.DieId) && !string.IsNullOrWhiteSpace(d.DieId) && d.DieId == quoteLineItem.HobNoShapeID))
                                    {
                                        DieList die = _nat01Context.DieList.First(d => !string.IsNullOrEmpty(d.DieId) && !string.IsNullOrWhiteSpace(d.DieId) && d.DieId == quoteLineItem.HobNoShapeID);
                                        // Machine Number Exists
                                        if (_nat01Context.MachineList.Any(m => quoteLineItem.MachineNo != null && quoteLineItem.MachineNo > 0 && m.MachineNo == quoteLineItem.MachineNo))
                                        {
                                            MachineList machine = _nat01Context.MachineList.First(m => quoteLineItem.MachineNo != null && quoteLineItem.MachineNo > 0 && m.MachineNo == quoteLineItem.MachineNo);
                                            // Is in shallow groove range
                                            if (machine.Od < .946 && die.LengthMajorAxis > .625)
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
                                            // Is BBS
                                            if (machine.MachineTypePrCode == "BBS")
                                            {
                                                float dieOD = (die.RefOutsideDiameter ?? 0);
                                                float dieWidth = (die.WidthMinorAxis ?? 0);
                                                // Korsch or Kilian
                                                if (machine.Description.Contains("KORSCH") || machine.Description.Contains("KILIAN"))
                                                {
                                                    // Too Large for press
                                                    if (dieOD > .4724 || dieWidth > .4331)
                                                    {
                                                        errors.Add("Tablet is too large for KORSCH/KILIAN BBS die.");
                                                    }
                                                }
                                                // Regular BBS
                                                else
                                                {
                                                    // Too Large for press
                                                    if (dieOD > .5 || dieWidth > .4724)
                                                    {
                                                        errors.Add("Tablet is too large for BBS die.");
                                                    }
                                                }
                                            }
                                        }

                                        // Exotic or Semi-exotic hob for this die number
                                        if (_nat01Context.HobList.Any(h => h.DieId == die.DieId && die.ShapeId != 1 && die.ShapeId != 2 && die.ShapeId != 3 && die.ShapeId != 4 && die.ShapeId != 5
                                        && (h.Class == "EX" || h.Class == "SX")))
                                        {
                                            // Material is NOT A2 steel
                                            if (!quoteLineItem.Material.Contains("A2"))
                                            {
                                                errors.Add("Dies are required to be A2 steel if the hob is exotic or semi-exotic.");
                                            }
                                        }
                                    }
                                    else
                                    {
                                        errors.Add("'D' has incorrect Die Number");
                                    }

                                    // Has Inserts
                                    if (ContainsAny(string.Join(",", quoteLineItem.OptionNumbers), new List<string> { "491", "492", "493", "494", "495", "496", "497" }, StringComparison.CurrentCulture))
                                    {
                                        // Die Groove W/O Relief
                                        if (quoteLineItem.OptionNumbers.Contains("437"))
                                        {
                                            errors.Add("'D' cannot have groove without relief and an insert.");
                                        }

                                        // Carbide || Ceramic
                                        if (ContainsAny(string.Join(",", quoteLineItem.OptionNumbers), new List<string> { "491", "492", "496" }, StringComparison.CurrentCulture))
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

                                        // Material without insert
                                        if (!quoteLineItem.Material.Contains("INSERT", StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            errors.Add("Please change material to include an insert.");
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    errors.Add("Failed checking for errors on '" + quoteLineItem.LineItemType + "'. Please let someone know.");
                                    WriteToErrorLog("QuoteErrors => Die", ex.Message, user);
                                }
                            }

                            // Die Segment
                            if (quoteLineItem.LineItemType == "DS")
                            {
                                try
                                {
                                    // Has multi-bore
                                    if (quoteLineItem.OptionNumbers.Contains("470"))
                                    {
                                        // Not Assembled
                                        if (!quoteLineItems.Any(qli =>
                                         qli.LineItemType == "UA" || qli.LineItemType == "UT" ||
                                         qli.LineItemType == "LA" || qli.LineItemType == "LT" ||
                                         qli.LineItemType == "LA" || qli.LineItemType == "LT"
                                        ))
                                        {
                                            // Not Solid Multi-Tip
                                            if (!quoteLineItems.Any(qli => (qli.TipQTY ?? 1) > 1))
                                            {
                                                // Machine Exists
                                                if (_nat01Context.MachineList.Any(m => quoteLineItem.MachineNo != null && quoteLineItem.MachineNo > 0 && m.MachineNo == quoteLineItem.MachineNo))
                                                {
                                                    MachineList machine = _nat01Context.MachineList.First(m => quoteLineItem.MachineNo != null && quoteLineItem.MachineNo > 0 && m.MachineNo == quoteLineItem.MachineNo);
                                                    short stations = machine.Stations ?? 1;
                                                    byte segmentQTY = machine.SegmentQty ?? 1;
                                                    // Stations / SegmentQTY has no remainder
                                                    if (stations % segmentQTY == 0 || stations == 1 || segmentQTY == 1)
                                                    {
                                                        byte boresPerSegment = (byte)(stations / segmentQTY);
                                                        // # of Multibores != boresPerSegment
                                                        try
                                                        {
                                                            if ((quoteLineItem.optionValuesO.First(qov => qov.OptionCode == "470").Integer ?? 1) != boresPerSegment)
                                                            {
                                                                errors.Add("'DS' has multi-bore option that does not equal (# of stations) / (# of segments).");
                                                            }
                                                        }
                                                        catch
                                                        {
                                                            errors.Add("'" + quoteLineItem.LineItemType + "' has option (470) without a value.");
                                                        }
                                                    }
                                                    else
                                                    {
                                                        errors.Add("Check that the number of stations and number of segments are correct.");
                                                    }
                                                }
                                            }
                                        }

                                    }
                                    else
                                    {
                                        errors.Add("Please check to see if 'DS' needs multi-bore (470).");
                                    }
                                    // Carbide Lined and not A2
                                    if (quoteLineItem.OptionNumbers.Contains("491") && !quoteLineItem.Material.Contains("A2"))
                                    {
                                        errors.Add("Carbide lined segments should be made out of A2 steel.");
                                    }
                                    // Die Number Exists
                                    if (!string.IsNullOrWhiteSpace(quoteLineItem.HobNoShapeID) && _nat01Context.DieList.Any(d => !string.IsNullOrEmpty(d.DieId) && !string.IsNullOrWhiteSpace(d.DieId) && d.DieId == quoteLineItem.HobNoShapeID))
                                    {

                                    }
                                    else
                                    {
                                        errors.Add("'DS' has incorrect Die Number");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    errors.Add("Failed checking for errors on '" + quoteLineItem.LineItemType + "'. Please let someone know.");
                                    WriteToErrorLog("QuoteErrors => Die Segment", ex.Message, user);
                                }
                            }

                            // Die and Die Segment
                            if (quoteLineItem.LineItemType == "D" || quoteLineItem.LineItemType == "DS")
                            {
                                try
                                {
                                    // W/ Insert
                                    if (quoteLineItem.Material.Contains("INSERT") || quoteLineItem.OptionNumbers.Contains("497"))
                                    {
                                        // No Insert Type
                                        if (!ContainsAny(string.Join(",", quoteLineItem.OptionNumbers), new List<string> { "491", "492", "493", "494", "495", "496" }, StringComparison.CurrentCulture))
                                        {
                                            errors.Add("Please specify the insert type for '" + quoteLineItem.LineItemType + "'. (Carbide, Ceramic, etc.)");
                                        }
                                    }

                                }
                                catch (Exception ex)
                                {
                                    errors.Add("Failed checking for errors on '" + quoteLineItem.LineItemType + "'. Please let someone know.");
                                    WriteToErrorLog("QuoteErrors => Die and Die Segment", ex.Message, user);
                                }
                            }

                            // Punches, Holders, and Assemblies
                            if (quoteLineItem.LineItemType == "U" || quoteLineItem.LineItemType == "UH" || quoteLineItem.LineItemType == "UA" ||
                                quoteLineItem.LineItemType == "L" || quoteLineItem.LineItemType == "LH" || quoteLineItem.LineItemType == "LCRP" || quoteLineItem.LineItemType == "LA" ||
                                quoteLineItem.LineItemType == "R" || quoteLineItem.LineItemType == "RH" || quoteLineItem.LineItemType == "RA")
                            {
                                try
                                {
                                    // Uppper or Upper Holder
                                    if (quoteLineItem.LineItemType == "U" || quoteLineItem.LineItemType == "UH")
                                    {
                                        // Hob Exists
                                        if (!string.IsNullOrWhiteSpace(quoteLineItem.HobNoShapeID) && _nat01Context.HobList.Any(h => h.HobNo == quoteLineItem.HobNoShapeID && h.TipQty == (quoteLineItem.TipQTY ?? 1) && h.BoreCircle == (quoteLineItem.BoreCircle ?? 0)))
                                        {
                                            HobList hob = _nat01Context.HobList.First(h => h.HobNo == quoteLineItem.HobNoShapeID && h.TipQty == (quoteLineItem.TipQTY ?? 1) && h.BoreCircle == (quoteLineItem.BoreCircle ?? 0));
                                            // Die Exists
                                            if (_nat01Context.DieList.Any(d => d.DieId == hob.DieId))
                                            {
                                                DieList die = _nat01Context.DieList.First(d => d.DieId == hob.DieId);
                                                // Will require key on rotary press
                                                if ((die.ShapeId ?? 99) > 1 || (quoteLineItem.TipQTY ?? 1) > 1)
                                                {
                                                    // Not Single Station machine type
                                                    if (!(quoteLineItem.MachinePriceCode == "E/F" || quoteLineItem.MachinePriceCode == "R" || quoteLineItem.MachinePriceCode == "R4"))
                                                    {
                                                        // Machine Exists
                                                        if (_nat01Context.MachineList.Any(m => quoteLineItem.MachineNo != null && quoteLineItem.MachineNo > 0 && m.MachineNo == quoteLineItem.MachineNo))
                                                        {
                                                            MachineList machine = _nat01Context.MachineList.First(m => quoteLineItem.MachineNo != null && quoteLineItem.MachineNo > 0 && m.MachineNo == quoteLineItem.MachineNo);
                                                            // Not Single Station
                                                            if ((machine.Stations ?? 2) > 1)
                                                            {
                                                                // Not keyed
                                                                if (!ContainsAny(string.Join(",", quoteLineItem.OptionNumbers), new List<string> { "130", "131", "132", "133", "139", "140", "141", "144" }, StringComparison.CurrentCulture))
                                                                {
                                                                    errors.Add("'" + quoteLineItem.LineItemType + "' Needs a key.");
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    // Upper or Upper Assembly
                                    if (quoteLineItem.LineItemType == "U" || quoteLineItem.LineItemType == "UA")
                                    {
                                        // Natoli Deep Fill
                                        if (quoteLineItem.MachineDescription.Contains("DEEP") && quoteLineItem.MachineDescription.Contains("FILL") && quoteLineItem.MachineDescription.Contains("NATOLI"))
                                        {
                                            // Special tip straight is not .75"
                                            if (!quoteLineItem.OptionNumbers.Contains("217") && !quoteLineItem.optionValuesA.Any(ov => ov.OptionCode == "217" && Math.Round((decimal)ov.Number1, 3) == (decimal).750))
                                            {
                                                errors.Add("Upper or Upper Assembly should have a tip straight of .75\"");
                                            }
                                        }
                                    }

                                    // Reject Punch or Reject Assembly
                                    if (quoteLineItem.LineItemType == "R" || quoteLineItem.LineItemType == "RA")
                                    {
                                        // No Short or Long Reject Option
                                        if (!quoteLineItem.OptionNumbers.Contains("350") && !quoteLineItem.OptionNumbers.Contains("354"))
                                        {
                                            errors.Add("'" + quoteLineItem.LineItemType + "', line item number " + quoteLineItem.LineItemNumber + " needs short or long reject option.");
                                        }
                                    }


                                    // Has Key Angle
                                    if (quoteLineItem.OptionNumbers.Contains("155"))
                                    {
                                        QuoteOptionValueGDegrees quoteOptionValueG = null;
                                        try
                                        {
                                            quoteOptionValueG = quoteLineItem.optionValuesG.First(qov => qov.OptionCode == "155");
                                        }
                                        catch
                                        {
                                            errors.Add("'" + quoteLineItem.LineItemType + "' has option (155) without a value.");
                                        }
                                short? angle = quoteOptionValueG.Degrees;
                                        string text = (quoteOptionValueG.Text ?? "").Trim();
                                        if (angle == null)
                                        {
                                            errors.Add("'" + quoteLineItem.LineItemType + "' needs a key angle value.");
                                        }
                                        if (quoteKeyAngle == null)
                                        {
                                            quoteKeyAngle = angle;
                                        }
                                        else
                                        {
                                            // Key angles do not match
                                            if (quoteKeyAngle != angle)
                                            {
                                                errors.Add("Key angles from different lines do not match.");
                                            }
                                        }
                                        // Customer Machine Exists
                                        if (_nat01Context.CustomerMachines.Any(m => quoteLineItem.MachineNo != null && quoteLineItem.MachineNo > 0 && m.MachineNo == quoteLineItem.MachineNo && !string.IsNullOrEmpty(m.CustomerNo) && m.CustAddressCode != null && m.CustomerNo.Trim() == quote.UserAcctNo.Trim() && m.CustAddressCode.Trim() == quote.UserLocNo.Trim()))
                                        {
                                            CustomerMachines customerMachine = _nat01Context.CustomerMachines.First(m => quoteLineItem.MachineNo != null && quoteLineItem.MachineNo > 0 && m.MachineNo == quoteLineItem.MachineNo && !string.IsNullOrEmpty(m.CustomerNo) && m.CustAddressCode != null && m.CustomerNo.Trim() == quote.UserAcctNo.Trim() && m.CustAddressCode.Trim() == quote.UserLocNo.Trim());
                                            // Is Lower Type
                                            if (quoteLineItem.LineItemType.Contains("L"))
                                            {
                                                if (customerMachine.LowerKeyAngle == null)
                                                {
                                                    errors.Add("'" + quoteLineItem.LineItemType + "' Key Angle in Customer Machines is blank");
                                                }
                                                if (customerMachine.LowerKeyAngle != 0 && string.IsNullOrEmpty(customerMachine.LowerKeyDirection))
                                                {
                                                    errors.Add("'" + quoteLineItem.LineItemType + "' Key Direction in Customer Machines is blank");
                                                }
                                                if (customerMachine.LowerKeyAngle != angle || (!string.IsNullOrEmpty(customerMachine.LowerKeyDirection) && customerMachine.LowerKeyDirection.Trim() != text))
                                                {
                                                    errors.Add("'" + quoteLineItem.LineItemType + "' Key Angle or Direction does not match Customer Machine.");
                                                }
                                            }
                                            else
                                            {
                                                if (customerMachine.KeyAngle == null)
                                                {
                                                    errors.Add("'" + quoteLineItem.LineItemType + "' Key Angle in Customer Machines is blank");
                                                }
                                                if (customerMachine.KeyAngle != 0 && string.IsNullOrEmpty(customerMachine.KeyDirection))
                                                {
                                                    errors.Add("'" + quoteLineItem.LineItemType + "' Key Direction in Customer Machines is blank");
                                                }
                                                if (customerMachine.KeyAngle != angle || (!string.IsNullOrEmpty(customerMachine.KeyDirection) && customerMachine.KeyDirection.Trim() != text))
                                                {
                                                    errors.Add("'" + quoteLineItem.LineItemType + "' Key Angle or Direction does not match Customer Machine.");
                                                }
                                            }
                                        }

                                    }

                                    // Machine Exists
                                    if (_nat01Context.MachineList.Any(m => quoteLineItem.MachineNo != null && quoteLineItem.MachineNo > 0 && m.MachineNo == quoteLineItem.MachineNo))
                                    {
                                        MachineList machine = _nat01Context.MachineList.First(m => quoteLineItem.MachineNo != null && quoteLineItem.MachineNo > 0 && m.MachineNo == quoteLineItem.MachineNo);
                                        // Not Standard
                                        if (!string.IsNullOrEmpty(machine.MachineTypePrCode) && machine.MachineTypePrCode.Trim() != "B" && machine.MachineTypePrCode.Trim() != "BB" && machine.MachineTypePrCode.Trim() != "BBS" && machine.MachineTypePrCode.Trim() != "D" && machine.MachineTypePrCode.Trim() != "D/B" && machine.MachineTypePrCode.Trim() != "B/D" && machine.MachineTypePrCode.Trim() != "FS12" && machine.MachineTypePrCode.Trim() != "DRY")
                                        {
                                            // Standard OL
                                            if (quoteLineItem.OptionNumbers.Contains("333") || quoteLineItem.OptionNumbers.Contains("332"))
                                            {
                                                errors.Add("'" + quoteLineItem.LineItemType + "' has TSM or EU length option and may not need it.");
                                            }
                                        }
                                        // Is Keyed
                                        if (ContainsAny(string.Join(",", quoteLineItem.OptionNumbers), new List<string> { "130", "131", "132", "133", "139", "140", "141", "144" }, StringComparison.CurrentCulture))
                                        {
                                            // No key line item
                                            if (!quoteLineItems.Any(qli => qli.LineItemType == "K"))
                                            {
                                                // Special key size
                                                if (quoteLineItem.OptionNumbers.Contains("151"))
                                                {
                                                    QuoteOptionValueBDoubleNum keySizeOptionValue = null;
                                                    try
                                                    {
                                                        keySizeOptionValue = quoteLineItem.optionValuesB.First(qov => qov.OptionCode == "151");
                                                    }
                                                    catch
                                                    {
                                                        errors.Add("'" + quoteLineItem.LineItemType + "' has option (215) without a value.");
                                                    }
                                                    double width = Math.Round(keySizeOptionValue.Number1 ?? 0, 4);
                                                    double length = Math.Round(keySizeOptionValue.Number2 ?? 0, 4);
                                                    // Key exists in table
                                                    if (!(width == .1865 && length == .75) && _nat01Context.Keys.Any(k => Math.Round(k.Width, 4) == width && Math.Round(k.Length, 4) == length))
                                                    {
                                                        try
                                                        {
                                                            Keys key = _nat01Context.Keys.Single(k => Math.Round(k.Width, 4) == width && Math.Round(k.Length, 4) == length);
                                                            // Key is not standard
                                                            if (!string.IsNullOrEmpty(key.DrawingNo) && !App.StandardKeys.Contains(key.DrawingNo.Trim()))
                                                            {
                                                                if (!quoteLineItems.Any(qli => qli.LineItemType == "K"))
                                                                {
                                                                    errors.Add("Key (" + key.DrawingNo.Trim() + ") on '" + quoteLineItem.LineItemType + "' needs a key line item.");
                                                                }
                                                            }
                                                        }
                                                        catch (InvalidOperationException)
                                                        {
                                                            errors.Add("More than one key exist at that size. Make sure that your key number is correct and a Key Line Item is added if we do not keep it in stock.");
                                                        }
                                                    }
                                                    else
                                                    {
                                                        errors.Add("No keys with key size: " + width + " X " + length + " exist in the Keys table.");
                                                    }
                                                }
                                            }
                                            // Is segmented machine
                                            if (machine.DieSegments == true && (machine.SegmentQty ?? 0) > 0)
                                            {
                                                // Does not have 100% interx
                                                if (!quoteLineItem.OptionNumbers.Contains("160"))
                                                {
                                                    errors.Add("'" + quoteLineItem.LineItemType + "' needs KEY 100% INTERCHANGEABLE WITHIN SET (160) to be used on segment machine.");
                                                }
                                                // Does not have segment key option
                                                if (!quoteLineItem.OptionNumbers.Contains("159"))
                                                {
                                                    errors.Add("'" + quoteLineItem.LineItemType + "' needs KEY FOR USE ON SEGMENTED TURRET (159) to be used on segment machine.");
                                                }
                                            }
                                            // No Key Angle
                                            if (!ContainsAny(string.Join(",", quoteLineItem.OptionNumbers), new List<string> { "155", "156" }, StringComparison.CurrentCulture))
                                            {
                                                // Has Hob
                                                if (!string.IsNullOrWhiteSpace(quoteLineItem.HobNoShapeID) && _nat01Context.HobList.Any(h => h.HobNo == quoteLineItem.HobNoShapeID && h.TipQty == (quoteLineItem.TipQTY ?? 1) && h.BoreCircle == (quoteLineItem.BoreCircle ?? 0)))
                                                {
                                                    HobList hob = _nat01Context.HobList.First(h => h.HobNo == quoteLineItem.HobNoShapeID && h.TipQty == (quoteLineItem.TipQTY ?? 1) && h.BoreCircle == (quoteLineItem.BoreCircle ?? 0));
                                                    if (_nat01Context.DieList.Any(d => !string.IsNullOrEmpty(d.DieId) && d.DieId.Trim() == hob.DieId.Trim()))
                                                    {
                                                        DieList die = _nat01Context.DieList.First(d => d.DieId.Trim() == hob.DieId.Trim());

                                                        // NOT round
                                                        if (die.ShapeId != 1)
                                                        {
                                                            if (!quoteLineItem.LineItemType.Contains("L"))
                                                            {
                                                                errors.Add("'" + quoteLineItem.LineItemType + "' is missing a key angle.");
                                                            }
                                                        }
                                                    }
                                                }


                                            }
                                            // Woodruff Key
                                            if (ContainsAny(string.Join(",", quoteLineItem.OptionNumbers), new List<string> { "130", "131", "132", "133" }, StringComparison.CurrentCulture))
                                            {
                                                // PMM4 STEEL
                                                if (quoteLineItem.Material.Contains("PM") && quoteLineItem.Material.Contains("M4"))
                                                {
                                                    errors.Add("'" + quoteLineItem.LineItemType + "' cannot have woodruff key and be PM M4 STEEL.");
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        errors.Add("'" + quoteLineItem.LineItemType + "' does not have a machine that exists in the Machine List.");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    errors.Add("Failed checking for errors on '" + quoteLineItem.LineItemType + "'. Please let someone know.");
                                    WriteToErrorLog("QuoteErrors => Punches, Holders, and Assemblies", ex.Message, user);
                                }
                            }

                            // Punches, Holders, and Heads
                            if (quoteLineItem.LineItemType == "U" || quoteLineItem.LineItemType == "UH" || quoteLineItem.LineItemType == "UHD" ||
                            quoteLineItem.LineItemType == "L" || quoteLineItem.LineItemType == "LH" || quoteLineItem.LineItemType == "LCRP" || quoteLineItem.LineItemType == "LHD" ||
                            quoteLineItem.LineItemType == "R" || quoteLineItem.LineItemType == "RH" || quoteLineItem.LineItemType == "RHD")
                            {
                                try
                                {
                                    // Machine Exists
                                    if (_nat01Context.MachineList.Any(m => quoteLineItem.MachineNo != null && quoteLineItem.MachineNo > 0 && m.MachineNo == quoteLineItem.MachineNo))
                                    {
                                        MachineList machine = _nat01Context.MachineList.First(m => quoteLineItem.MachineNo != null && quoteLineItem.MachineNo > 0 && m.MachineNo == quoteLineItem.MachineNo);
                                        // NOT Single Station or B or D
                                        if (machine.Stations != 1 ||
                                            (((machine.UpperSize.Trim() ?? machine.LowerSize.Trim()) != @"3/4 x 5-3/4" && (machine.MachineTypePrCode.Trim() == "B" || machine.MachineTypePrCode.Trim() == "BB" || machine.MachineTypePrCode.Trim() == "BBS" ||
                                            ((machine.MachineTypePrCode.Trim() == "ZZZ" || machine.MachineTypePrCode.Trim() == "DRY") && (machine.UpperSize.Trim() ?? machine.LowerSize.Trim()) == @"1 x 5-3/4")))
                                            ||
                                            (machine.MachineTypePrCode.Trim() == "D" ||
                                                ((machine.MachineTypePrCode.Trim() == "ZZZ" || machine.MachineTypePrCode.Trim() == "DRY") && (machine.UpperSize.Trim() ?? machine.LowerSize.Trim()) == @"1-1/4 x 5-3/4") ||
                                                machine.MachineNo == 1015))
                                            )
                                        {
                                            // No Head Option
                                            if (!ContainsAny(string.Join(",", quoteLineItem.OptionNumbers), new List<string> { "001", "002", "003", "004", "005", "006", "007", "008", "009", "010", "011", "012", "013", "014", "015", "016", "017", "018", "019", "022", "024", "025" }, StringComparison.CurrentCulture))
                                            {
                                                errors.Add("'" + quoteLineItem.LineItemType + "' is missing a head type option.");
                                            }
                                        }


                                        // Machine is D and NOT EU1-441
                                        if ((machine.MachineTypePrCode.Trim() == "D" || ((machine.MachineTypePrCode.Trim() == "ZZZ" || machine.MachineTypePrCode.Trim() == "DRY") && (machine.UpperSize.Trim() ?? machine.LowerSize.Trim()) == @"1-1/4 x 5-3/4") || machine.MachineNo == 1015) &&
                                            (machine.UpperSize.Trim() ?? machine.LowerSize.Trim()) != @"1-1/2 x 5-3/4")
                                        {
                                            // Has 441 style head option
                                            if (ContainsAny(string.Join(",", quoteLineItem.OptionNumbers), new List<string> { "007", "011", "013", "014", "018" }, StringComparison.CurrentCulture))
                                            {
                                                errors.Add("'" + quoteLineItem.LineItemType + "' has a 441 style head but the machine is a normal 'D' machine.");
                                            }
                                        }
                                        // Machine is D and EU1-441
                                        else if ((machine.MachineTypePrCode.Trim() == "D" || ((machine.MachineTypePrCode.Trim() == "ZZZ" || machine.MachineTypePrCode.Trim() == "DRY") && (machine.UpperSize.Trim() ?? machine.LowerSize.Trim()) == @"1-1/4 x 5-3/4") || machine.MachineNo == 1015) &&
                                                (machine.UpperSize.Trim() ?? machine.LowerSize.Trim()) == @"1-1/2 x 5-3/4")
                                        {
                                            // Does not have 441 head option or special
                                            if (!ContainsAny(string.Join(",", quoteLineItem.OptionNumbers), new List<string> { "007", "008", "011", "013", "014", "018" }, StringComparison.CurrentCulture))
                                            {
                                                errors.Add("'" + quoteLineItem.LineItemType + "' does not have a 441 style head but the machine is of 441 type.");
                                            }
                                        }

                                        // Machine is B and NOT FS19
                                        if (((machine.UpperSize.Trim() ?? machine.LowerSize.Trim()) != @"3/4 x 5-3/4" && (machine.MachineTypePrCode.Trim() == "B" || machine.MachineTypePrCode.Trim() == "BB" || machine.MachineTypePrCode.Trim() == "BBS" || ((machine.MachineTypePrCode.Trim() == "ZZZ" || machine.MachineTypePrCode.Trim() == "DRY") && (machine.UpperSize.Trim() ?? machine.LowerSize.Trim()) == @"1 x 5-3/4"))) &&
                                            (machine.UpperSize.Trim() ?? machine.LowerSize.Trim()) != @"1-1/4 x 5-3/4")
                                        {
                                            // Has FS-19 style head option
                                            if (ContainsAny(string.Join(",", quoteLineItem.OptionNumbers), new List<string> { "016", "019" }, StringComparison.CurrentCulture))
                                            {
                                                errors.Add("'" + quoteLineItem.LineItemType + "' has an FS-19 style head but the machine is a normal 'B' machine.");
                                            }
                                        }
                                        // Machine is B and FS-19
                                        else if (((machine.UpperSize.Trim() ?? machine.LowerSize.Trim()) != @"3/4 x 5-3/4" && (machine.MachineTypePrCode.Trim() == "B" || machine.MachineTypePrCode.Trim() == "BB" || machine.MachineTypePrCode.Trim() == "BBS" || ((machine.MachineTypePrCode.Trim() == "ZZZ" || machine.MachineTypePrCode.Trim() == "DRY") && (machine.UpperSize.Trim() ?? machine.LowerSize.Trim()) == @"1 x 5-3/4"))) &&
                                            (machine.UpperSize.Trim() ?? machine.LowerSize.Trim()) != @"1-1/4 x 5-3/4")
                                        {
                                            // Does not have FS-19 head option or special
                                            if (!ContainsAny(string.Join(",", quoteLineItem.OptionNumbers), new List<string> { "008", "016", "019" }, StringComparison.CurrentCulture))
                                            {
                                                errors.Add("'" + quoteLineItem.LineItemType + "' does not have an FS-19 style head but the machine is of FS-19 type.");
                                            }
                                        }

                                        // Machine is B and NOT FS-12
                                        if (((machine.UpperSize.Trim() ?? machine.LowerSize.Trim()) != @"3/4 x 5-3/4" && (machine.MachineTypePrCode.Trim() == "B" || machine.MachineTypePrCode.Trim() == "BB" || machine.MachineTypePrCode.Trim() == "BBS" || ((machine.MachineTypePrCode.Trim() == "ZZZ" || machine.MachineTypePrCode.Trim() == "DRY") && (machine.UpperSize.Trim() ?? machine.LowerSize.Trim()) == @"1 x 5-3/4"))))
                                        {
                                            // Has FS-12 style head option
                                            if (ContainsAny(string.Join(",", quoteLineItem.OptionNumbers), new List<string> { "017" }, StringComparison.CurrentCulture))
                                            {
                                                errors.Add("'" + quoteLineItem.LineItemType + "' has an FS-12 style head but the machine is a normal 'B' machine.");
                                            }
                                        }
                                        // Machine is B and FS-12
                                        else if (((machine.UpperSize.Trim() ?? machine.LowerSize.Trim()) == @"3/4 x 5-3/4" && (machine.MachineTypePrCode.Trim() == "B" || machine.MachineTypePrCode.Trim() == "BB" || machine.MachineTypePrCode.Trim() == "BBS" || ((machine.MachineTypePrCode.Trim() == "ZZZ" || machine.MachineTypePrCode.Trim() == "DRY") && (machine.UpperSize.Trim() ?? machine.LowerSize.Trim()) == @"1 x 5-3/4"))))
                                        {
                                            // Does not have FS-12 head option or special
                                            if (!ContainsAny(string.Join(",", quoteLineItem.OptionNumbers), new List<string> { "008", "017" }, StringComparison.CurrentCulture))
                                            {
                                                errors.Add("'" + quoteLineItem.LineItemType + "' does not have an FS-12 style head but the machine is of FS-12 type.");
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    errors.Add("Failed checking for errors on '" + quoteLineItem.LineItemType + "'. Please let someone know.");
                                    WriteToErrorLog("QuoteErrors => Punches, Holders, and Heads", ex.Message, user);
                                }
                            }

                            // Punches, Holders, and Caps
                            if (quoteLineItem.LineItemType == "U" || quoteLineItem.LineItemType == "UH" || quoteLineItem.LineItemType == "UC" ||
                                quoteLineItem.LineItemType == "L" || quoteLineItem.LineItemType == "LH" || quoteLineItem.LineItemType == "LC" ||
                                quoteLineItem.LineItemType == "R" || quoteLineItem.LineItemType == "RH" || quoteLineItem.LineItemType == "RC")
                            {
                                try
                                {
                                    // Machine Exists
                                    if (_nat01Context.MachineList.Any(m => quoteLineItem.MachineNo != null && quoteLineItem.MachineNo > 0 && m.MachineNo == quoteLineItem.MachineNo))
                                    {
                                        MachineList machine = _nat01Context.MachineList.First(m => quoteLineItem.MachineNo != null && quoteLineItem.MachineNo > 0 && m.MachineNo == quoteLineItem.MachineNo);
                                        // Stokes machine
                                        if (machine.Description.Contains("STOKES"))
                                        {
                                            // Is Grooved
                                            if (ContainsAny(string.Join(",", quoteLineItem.OptionNumbers), new List<string> { "110", "111", "112", "113", "115", "116" }, StringComparison.CurrentCulture))
                                            {
                                                errors.Add("Grooves on STOKES machines are not recommended.");
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    errors.Add("Failed checking for errors on '" + quoteLineItem.LineItemType + "'. Please let someone know.");
                                    WriteToErrorLog("QuoteErrors => Punches, Holdersm and Caps", ex.Message, user);
                                }

                            }

                            // Punches and Holders
                            if (quoteLineItem.LineItemType == "U" || quoteLineItem.LineItemType == "UH" ||
                                quoteLineItem.LineItemType == "L" || quoteLineItem.LineItemType == "LH" || quoteLineItem.LineItemType == "LCRP" ||
                                quoteLineItem.LineItemType == "R" || quoteLineItem.LineItemType == "RH")
                            {
                                try
                                {
                                    // Hold low barrel tol & not a Fette or Korsch
                                    if (!ContainsAny(quoteLineItem.MachineDescription, new List<string> { "FETTE", "KORSCH" }, StringComparison.InvariantCultureIgnoreCase) && quoteLineItem.OptionNumbers.Contains("123"))
                                    {
                                        errors.Add("'" + quoteLineItem.LineItemType + "' has hold low barrel tolerance. Hold low barrel tolerance may not be required for this machine.");
                                    }
                                    // Machine Exists
                                    if (_nat01Context.MachineList.Any(m => quoteLineItem.MachineNo != null && quoteLineItem.MachineNo > 0 && m.MachineNo == quoteLineItem.MachineNo))
                                    {
                                        MachineList machine = _nat01Context.MachineList.First(m => quoteLineItem.MachineNo != null && quoteLineItem.MachineNo > 0 && m.MachineNo == quoteLineItem.MachineNo);
                                        // Is B or D Machine
                                        if (((machine.UpperSize.Trim() ?? machine.LowerSize.Trim()) != @"3/4 x 5-3/4" && (machine.MachineTypePrCode.Trim() == "B" || machine.MachineTypePrCode.Trim() == "BB" || machine.MachineTypePrCode.Trim() == "BBS" ||
                                            ((machine.MachineTypePrCode.Trim() == "ZZZ" || machine.MachineTypePrCode.Trim() == "DRY") && (machine.UpperSize.Trim() ?? machine.LowerSize.Trim()) == @"1 x 5-3/4")))
                                            ||
                                            (machine.MachineTypePrCode.Trim() == "D" ||
                                                ((machine.MachineTypePrCode.Trim() == "ZZZ" || machine.MachineTypePrCode.Trim() == "DRY") && (machine.UpperSize.Trim() ?? machine.LowerSize.Trim()) == @"1-1/4 x 5-3/4") ||
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
                                                                    if (ContainsAny(string.Join(",", quoteLineItem.OptionNumbers), new List<string> { "333", "003", "004", "005", "006", "007", "009" }, StringComparison.CurrentCulture))
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
                                        // Not B or D Machine
                                        else
                                        {
                                            // Micro-Mirror Finish Barrels
                                            if (quoteLineItem.OptionNumbers.Contains("099"))
                                            {
                                                errors.Add("Cannot micro-mirror finish non B or D tools.");
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    errors.Add("Failed checking for errors on '" + quoteLineItem.LineItemType + "'. Please let someone know.");
                                    WriteToErrorLog("QuoteErrors => Punches and Holders", ex.Message, user);
                                }
                            }

                            // Punches and Tips
                            if (quoteLineItem.LineItemType == "U" || quoteLineItem.LineItemType == "UT" ||
                                quoteLineItem.LineItemType == "L" || quoteLineItem.LineItemType == "LCRP" || quoteLineItem.LineItemType == "LT" ||
                                quoteLineItem.LineItemType == "R" || quoteLineItem.LineItemType == "RT")
                            {
                                try
                                {
                                    // Tip
                                    if (quoteLineItem.LineItemType == "UT" || quoteLineItem.LineItemType == "LT" || quoteLineItem.LineItemType == "RT")
                                    {
                                        if (quoteLineItem.TipQTY > 1)
                                        {
                                            errors.Add("'" + quoteLineItem.LineItemType + "' has a multi-tip solid hob linked to it.");
                                        }
                                    }
                                    // Has Special tip relief
                                    if (quoteLineItem.OptionNumbers.Contains("215"))
                                    {

                                        QuoteOptionValueASingleNum quoteOptionValueASingleNum = null;
                                        try
                                        {
                                            quoteOptionValueASingleNum = quoteLineItem.optionValuesA.First(qov => qov.OptionCode == "215");
                                        }
                                        catch
                                        {
                                            errors.Add("'" + quoteLineItem.LineItemType + "' has option (215) without a value.");
                                        }
                                        // Relief is Diameter type callout
                                        if (quoteOptionValueASingleNum.Text.Contains("Diameter", StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            // Diameter is extremely small
                                            if ((quoteOptionValueASingleNum.Number1 ?? 0) < .07)
                                            {
                                                errors.Add("'" + quoteLineItem.LineItemType + "' has a special tip relief Diameter that seems too small. Perhaps you meant Per Side?");
                                            }
                                        }
                                        else
                                        {
                                            // Per Side is extremely large
                                            if ((quoteOptionValueASingleNum.Number1 ?? 100) > .07)
                                            {
                                                errors.Add("'" + quoteLineItem.LineItemType + "' has a special tip relief Per Side that seems too large. Perhaps you meant Diameter?");
                                            }
                                        }

                                    }
                                    // Has HobNo in HobList
                                    if (!string.IsNullOrWhiteSpace(quoteLineItem.HobNoShapeID) && _nat01Context.HobList.Any(h => h.HobNo == quoteLineItem.HobNoShapeID && h.TipQty == (quoteLineItem.TipQTY ?? 1) && h.BoreCircle == (quoteLineItem.BoreCircle ?? 0)))
                                    {
                                        HobList hob = _nat01Context.HobList.First(h => h.HobNo == quoteLineItem.HobNoShapeID && h.TipQty == (quoteLineItem.TipQTY ?? 1) && h.BoreCircle == (quoteLineItem.BoreCircle ?? 0));

                                        // Flat Face
                                        if (hob.CupDepth == 0 || hob.CupCode == 1)
                                        {
                                            if (hob.CupDepth != 0)
                                            {
                                                errors.Add("'" + quoteLineItem.LineItemType + "' has a cup code for a flat face, but the cup depth is not zero.");
                                            }
                                            if (hob.CupCode != 1)
                                            {
                                                errors.Add("'" + quoteLineItem.LineItemType + "' has a cup depth of zero, but the cup code is not for a flat face.");
                                            }
                                            if (quoteLineItem.OptionNumbers.Contains("338") || (quoteLineItem.OptionNumbers.Contains("334") && quoteLineItem.OptionNumbers.Contains("336")))
                                            {
                                                errors.Add("'" + quoteLineItem.LineItemType + "' is flat face but has options to tolerance the cup or both the overall length and working length.");
                                            }
                                        }

                                        // Hob not made
                                        if (!string.IsNullOrEmpty(hob.HobYorNorD) && hob.HobYorNorD.Trim() != "Y" && hob.HobYorNorD.Trim() != "M")
                                        {
                                            // No Hob Line Item Matching
                                            if (!quoteLineItems.Any(qli => qli.LineItemType == "H" && qli.HobNoShapeID == hob.HobNo && (qli.TipQTY ?? 1) == (hob.TipQty ?? 1) && (qli.BoreCircle ?? 0) == (hob.BoreCircle ?? 0)))
                                            {
                                                errors.Add("Hob Line Item may be required for Hob#: " + hob.HobNo + ", Tips:" + (hob.TipQty ?? 1) + ", and Bore Circle: " + (hob.BoreCircle ?? 0) + ".");
                                            }
                                        }

                                        // Has DieId in HobList
                                        if (!string.IsNullOrEmpty(hob.DieId))
                                        {
                                            // DieId in DieList
                                            if (_nat01Context.DieList.Any(d => !string.IsNullOrEmpty(d.DieId) && d.DieId.Trim() == hob.DieId.Trim()))
                                            {
                                                DieList die = _nat01Context.DieList.First(d => d.DieId.Trim() == hob.DieId.Trim());

                                                // Width is less than .25
                                                if (!string.IsNullOrEmpty(die.ShapeCode) && die.ShapeCode.Trim() == "1" && die.WidthMinorAxis < .25)
                                                {
                                                    if (!quoteLineItems.Any(qli => qli.OptionNumbers.Contains("222")))
                                                    {
                                                        errors.Add("Based on the tablet size, the tools may need shortened lower tips and undercut dies.");
                                                    }
                                                }

                                                // Cup Depth Incorrect
                                                if (hob.CupDepth > ((die.WidthMinorAxis - (hob.Land * 2)) / 2))
                                                {
                                                    errors.Add("'" + quoteLineItem.LineItemType + "' - " + hob.HobNo + " has incorrect cup depth in the database (Magic).");
                                                }

                                                // Has Hob
                                                if (quoteLineItems.Any(qli => qli.LineItemType == "H"))
                                                {
                                                    // Too large to hob
                                                    if ((die.WidthMinorAxis ?? 0) > 1.125 || (die.LengthMajorAxis ?? 0) > 1.125 || (die.RefOutsideDiameter ?? 0) > 1.125)
                                                    {
                                                        errors.Add("Tablet size is too large to hob.");
                                                    }
                                                    // Machine Exists
                                                    if (_nat01Context.MachineList.Any(m => quoteLineItem.MachineNo != null && quoteLineItem.MachineNo > 0 && m.MachineNo == quoteLineItem.MachineNo))
                                                    {
                                                        MachineList machine = _nat01Context.MachineList.First(m => quoteLineItem.MachineNo != null && quoteLineItem.MachineNo > 0 && m.MachineNo == quoteLineItem.MachineNo);
                                                        // Not ZZZ
                                                        if (machine.MachineTypePrCode != "ZZZ")
                                                        {
                                                            if (machine.UpperSize.Trim().Contains("x"))
                                                            {
                                                                string upperStockDiameter = machine.UpperSize.Trim().Remove(machine.UpperSize.Trim().IndexOf('x'));
                                                                if (upperStockDiameter.Contains("-"))
                                                                {
                                                                    string[] uSDSplit = upperStockDiameter.Split('-');
                                                                    double upperStockWholeNumber = Convert.ToDouble(uSDSplit[0]);
                                                                    if (uSDSplit[1].ToString().Contains("/"))
                                                                    {
                                                                        string[] fraction = uSDSplit[1].ToString().Split('/');
                                                                        double upperStockFraction = Convert.ToDouble(fraction[0]) / Convert.ToDouble(fraction[1]);
                                                                        if (upperStockWholeNumber + upperStockFraction > 2.125)
                                                                        {
                                                                            errors.Add("Tool is too large to hob.");
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                            if (machine.LowerSize.Trim().Contains("x"))
                                                            {
                                                                string lowerStockDiameter = machine.LowerSize.Trim().Remove(machine.LowerSize.Trim().IndexOf('x'));
                                                                if (lowerStockDiameter.Contains("-"))
                                                                {
                                                                    string[] lSDSplit = lowerStockDiameter.Split('-');
                                                                    double lowerStockWholeNumber = Convert.ToDouble(lSDSplit[0]);
                                                                    if (lSDSplit[1].ToString().Contains("/"))
                                                                    {
                                                                        string[] fraction = lSDSplit[1].ToString().Split('/');
                                                                        double lowerStockFraction = Convert.ToDouble(fraction[0]) / Convert.ToDouble(fraction[1]);
                                                                        if (lowerStockWholeNumber + lowerStockFraction > 2.125)
                                                                        {
                                                                            errors.Add("Tool is too large to hob.");
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }

                                                double? punchWidth = null;
                                                double? punchLength = null;
                                                // Special tip size
                                                if (quoteLineItem.OptionNumbers.Contains("200"))
                                                {
                                                    try
                                                    {
                                                        punchWidth = quoteLineItem.optionValuesB.First(qov => qov.OptionCode == "200").Number1;
                                                        punchLength = quoteLineItem.optionValuesB.First(qov => qov.OptionCode == "200").Number2;
                                                    }
                                                    catch
                                                    {
                                                        errors.Add("'" + quoteLineItem.LineItemType + "' has option (200) without a value.");
                                                    }
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
                                                        if ((machine.UpperSize.Trim() ?? machine.LowerSize.Trim()) != @"3/4 x 5-3/4" && (machine.MachineTypePrCode.Trim() == "B" || machine.MachineTypePrCode.Trim() == "BB" || machine.MachineTypePrCode.Trim() == "BBS" ||
                                                                           ((machine.MachineTypePrCode.Trim() == "ZZZ" || machine.MachineTypePrCode.Trim() == "DRY") && (machine.UpperSize.Trim() ?? machine.LowerSize.Trim()) == @"1 x 5-3/4"))
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
                                                            ((machine.MachineTypePrCode.Trim() == "ZZZ" || machine.MachineTypePrCode.Trim() == "DRY") && (machine.UpperSize.Trim() ?? machine.LowerSize.Trim()) == @"1-1/4 x 5-3/4") ||
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
                                                // Has Die/Die Segment Line Item
                                                if (quoteLineItems.Any(qli => qli.LineItemType == "D" || qli.LineItemType == "DS"))
                                                {
                                                    QuoteLineItem dieQuoteLineItem = quoteLineItems.First(qli => qli.LineItemType == "D" || qli.LineItemType == "DS");

                                                    // Special bore size
                                                    if (dieQuoteLineItem.OptionNumbers.Contains("425"))
                                                    {
                                                        try
                                                        {
                                                            dieWidth = quoteLineItem.optionValuesB.First(qov => qov.OptionCode == "425").Number1;
                                                            dieLength = quoteLineItem.optionValuesB.First(qov => qov.OptionCode == "425").Number2;
                                                        }
                                                        catch
                                                        {
                                                            errors.Add("'" + quoteLineItem.LineItemType + "' has option (425) without a value.");
                                                        }
                                                    }
                                                    else
                                                    {
                                                        dieWidth = die.WidthMinorAxis;
                                                        dieLength = die.LengthMajorAxis;
                                                        if (_nat01Context.MachineList.Any(m => m.MachineNo == quoteLineItem.MachineNo))
                                                        {
                                                            MachineList machine = _nat01Context.MachineList.First(m => m.MachineNo == quoteLineItem.MachineNo);
                                                            // Is B Machine
                                                            if ((machine.UpperSize.Trim() ?? machine.LowerSize.Trim()) != @"3/4 x 5-3/4" && (machine.MachineTypePrCode.Trim() == "B" || machine.MachineTypePrCode.Trim() == "BB" || machine.MachineTypePrCode.Trim() == "BBS" ||
                                                                               ((machine.MachineTypePrCode.Trim() == "ZZZ" || machine.MachineTypePrCode.Trim() == "DRY") && (machine.UpperSize.Trim() ?? machine.LowerSize.Trim()) == @"1 x 5-3/4"))
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
                                                                ((machine.MachineTypePrCode.Trim() == "ZZZ" || machine.MachineTypePrCode.Trim() == "DRY") && (machine.UpperSize.Trim() ?? machine.LowerSize.Trim()) == @"1-1/4 x 5-3/4") ||
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

                                                    if ((punchWidth ?? 0) > (dieWidth ?? ((punchWidth ?? 0) + 1)) || ((punchLength ?? -1) > (dieLength ?? (punchLength ?? -1) + 1)))
                                                    {
                                                        errors.Add("Tip sizes are larger than die bore.");
                                                    }
                                                    if (punchWidth + (quoteLineItem.LineItemType.Contains("L") ? .0035 : .0045) < dieWidth || (dieLength == null || dieLength == 0 ? false : punchLength + (quoteLineItem.LineItemType.Contains("L") ? .0035 : .0045) < dieLength))
                                                    {
                                                        errors.Add("Tip sizes seem too small compared to the die bore.");
                                                    }
                                                }
                                                dieWidth = dieWidth ?? die.WidthMinorAxis;
                                                dieLength = dieLength ?? die.LengthMajorAxis;
                                                // Less than .001" clearance
                                                decimal clearance = Math.Round((decimal)dieWidth, 4) - Math.Round((decimal)punchWidth, 4);
                                                if (clearance < (decimal).001)
                                                {
                                                    // No hold low tip size toleracne
                                                    if (!quoteLineItem.OptionNumbers.Contains("207"))
                                                    {
                                                        errors.Add("'" + quoteLineItem.LineItemType + "' should have HOLD LOW TIP SIZE TOLERANCE (207) because the clearance is " + clearance + "\"");
                                                    }
                                                    // No tip concentricity or 100% interx
                                                    if (!quoteLineItem.OptionNumbers.Contains("202") && !quoteLineItem.OptionNumbers.Contains("160"))
                                                    {
                                                        errors.Add("'" + quoteLineItem.LineItemType + "' should have SPECIAL TIP CONCENTRICITY (202) of LESS than or equal to " + clearance + "\". Please also add to die if ordered.");
                                                    }
                                                    // No tip concentricity, has 100% interx, but clearance is less than .0005"
                                                    if (!quoteLineItem.OptionNumbers.Contains("202") && quoteLineItem.OptionNumbers.Contains("160") && clearance < (decimal).0005)
                                                    {
                                                        errors.Add("'" + quoteLineItem.LineItemType + "' should have SPECIAL TIP CONCENTRICITY (202) of LESS than or equal to " + clearance + "\". Please also add to die if ordered.");
                                                    }
                                                    // Tip concentricity is not tight enough
                                                    if (quoteLineItem.OptionNumbers.Contains("202") && quoteLineItem.optionValuesA.Any(qov => qov.OptionCode == "202" && (decimal)qov.Number1 > clearance))
                                                    {
                                                        errors.Add("'" + quoteLineItem.LineItemType + "' should have SPECIAL TIP CONCENTRICITY (202) of LESS than or equal to " + clearance + "\". Please also add to die if ordered.");
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                errors.Add("'" + quoteLineItem.LineItemType + "' has a die number that is not in the Die List.");
                                            }
                                        }
                                        else
                                        {
                                            errors.Add("'" + quoteLineItem.LineItemType + "' hob (" + hob.HobNo + ") is assigned a die number that does not exist in the Die List.");
                                        }

                                        // Lower Punch
                                        if (quoteLineItem.LineItemType == "L")
                                        {
                                            // Is Bisected
                                            if (!ContainsAny(hob.BisectCode, new List<string> { "000", "010", "020", "030" }, StringComparison.CurrentCulture))
                                            {
                                                // Isn't Keyed
                                                if (!ContainsAny(string.Join(",", quoteLineItem.OptionNumbers), new List<string> { "130", "131", "132", "133", "139", "140", "141", "144" }, StringComparison.CurrentCulture))
                                                {
                                                    // Die Exists
                                                    if (_nat01Context.DieList.Any(d => !string.IsNullOrEmpty(d.DieId) && d.DieId.Trim() == hob.DieId.Trim()))
                                                    {
                                                        DieList die = _nat01Context.DieList.First(d => d.DieId.Trim() == hob.DieId.Trim());
                                                        // Is Round
                                                        if (die.ShapeId == 1)
                                                        {
                                                            errors.Add("'L' is bisected. Please add key at appropriate take-off angle.");
                                                        }
                                                    }
                                                }
                                                // Is Keyed
                                                else
                                                {
                                                    // Key Angle is 0°
                                                    if (quoteLineItem.OptionNumbers.Contains("156") || (quoteLineItem.OptionNumbers.Contains("155") && quoteLineItem.optionValuesG.Any(qov => qov.Degrees == 0)))
                                                    {
                                                        errors.Add("'L' is bisected. Please check that the key is oriented for proper take-off.");
                                                    }
                                                }
                                            }

                                            // Is Reduced Tip Width || Is Strengthened Tip || Is Solid MultiTip || Carbide Tipped
                                            if ((quoteLineItem.OptionNumbers.Contains("204") && quoteLineItem.optionValuesA.Any(qo => qo.OptionCode == "204" && qo.Number1 < .1875)) ||
                                            quoteLineItem.OptionNumbers.Contains("222") ||
                                            quoteLineItem.TipQTY > 1 ||
                                            quoteLineItem.OptionNumbers.Contains("240"))
                                            {
                                                // Machine Number Exists
                                                if (_nat01Context.MachineList.Any(m => quoteLineItem.MachineNo != null && quoteLineItem.MachineNo > 0 && m.MachineNo == quoteLineItem.MachineNo))
                                                {
                                                    MachineList machine = _nat01Context.MachineList.First(m => quoteLineItem.MachineNo != null && quoteLineItem.MachineNo > 0 && m.MachineNo == quoteLineItem.MachineNo);
                                                    // Is B Machine
                                                    if ((machine.UpperSize.Trim() ?? machine.LowerSize.Trim()) != @"3/4 x 5-3/4" && (machine.MachineTypePrCode.Trim() == "B" || machine.MachineTypePrCode.Trim() == "BB" || machine.MachineTypePrCode.Trim() == "BBS" ||
                                                       ((machine.MachineTypePrCode.Trim() == "ZZZ" || machine.MachineTypePrCode.Trim() == "DRY") && (machine.UpperSize.Trim() ?? machine.LowerSize.Trim()) == @"1 x 5-3/4"))
                                                        )
                                                    {
                                                        // Has Special Barrel Diameter
                                                        if (quoteLineItem.OptionNumbers.Contains("120"))
                                                        {
                                                            // Barrel Diameter less than .7475
                                                            if (quoteLineItem.optionValuesA.Any(qo => qo.OptionCode == "120" && qo.Number1 < .7475))
                                                            {
                                                                errors.Add("'L' has reduced tip width, strengthened lower tip, multi-tipped, or carbide tipped. Check to see if increasing the barrel diameter is possible to improve stability.");
                                                            }
                                                        }
                                                        // Reduced tip width
                                                        else if (quoteLineItem.optionValuesA.Any(qo => qo.OptionCode == "204" && qo.Number1 < .1875))
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
                                                                    if (ContainsAny(string.Join(",", quoteLineItem.OptionNumbers), new List<string> { "333", "003", "004", "005", "006", "007", "009" }, StringComparison.CurrentCulture))
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
                                                        ((machine.MachineTypePrCode.Trim() == "ZZZ" || machine.MachineTypePrCode.Trim() == "DRY") && (machine.UpperSize.Trim() ?? machine.LowerSize.Trim()) == @"1-1/4 x 5-3/4") ||
                                                        machine.MachineNo == 1015)
                                                    {
                                                        // Has Special Barrel Diameter
                                                        if (quoteLineItem.OptionNumbers.Contains("120"))
                                                        {
                                                            // Barrel Diameter less than .9975
                                                            if (quoteLineItem.optionValuesA.Any(qo => qo.OptionCode == "120" && qo.Number1 < .9975))
                                                            {
                                                                errors.Add("'L' has reduced tip width, strengthened lower tip, multi-tipped, or carbide tipped. Check to see if increasing the barrel diameter is possible to improve stability.");
                                                            }
                                                        }
                                                        // Reduced tip width
                                                        else if (quoteLineItem.OptionNumbers.Contains("204") && quoteLineItem.optionValuesA.Any(qo => qo.OptionCode == "204" && qo.Number1 < .1875))
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
                                                                    if (ContainsAny(string.Join(",", quoteLineItem.OptionNumbers), new List<string> { "333", "003", "004", "005", "006", "007", "009" }, StringComparison.CurrentCulture))
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
                                catch (Exception ex)
                                {
                                    errors.Add("Failed checking for errors on '" + quoteLineItem.LineItemType + "'. Please let someone know.");
                                    WriteToErrorLog("QuoteErrors => Punches and Tips", ex.Message, user);
                                }
                            }
                            // Alignment Tool
                            if (quoteLineItem.LineItemType == "A")
                            {
                                try
                                {
                                    // Need Appropriate interchangeability options
                                    if (quoteLineItems.Any(qli => (qli.LineItemType == "U" || qli.LineItemType == "UH" || qli.LineItemType == "R" || qli.LineItemType == "RH") && !qli.OptionNumbers.Contains("160")))
                                    {
                                        errors.Add("'" + quoteLineItems.First(qli => (qli.LineItemType == "U" || qli.LineItemType == "UH" || qli.LineItemType == "R" || qli.LineItemType == "RH") && !qli.OptionNumbers.Contains("160")).LineItemType + "' needs (160) KEY 100% INTERCHANGEABLE WITHIN SET for 100% interchangeability with the Die Alignment Tool.");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    errors.Add("Failed checking for errors on '" + quoteLineItem.LineItemType + "'. Please let someone know.");
                                    WriteToErrorLog("QuoteErrors => Alignment Tool", ex.Message, user);
                                }

                            }

                        }
                    }
                    quote.Dispose();
                    _nat01Context.Dispose();
                    _nat02Context.Dispose();
                    _driveworksContext.Dispose();
                    if (errors.Count > 0)
                    {
                        errors.Sort();
                    }

                }
            }
            catch (Exception ex)
            {
                errors.Add("Could not finish checking for errors. Please let someone know that it failed.");
                WriteToErrorLog("QuoteErrors", ex.Message, user);
            }
            return errors;
        }
    }
}
